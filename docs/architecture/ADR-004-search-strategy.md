# ADR-004: Search Strategy - PostgreSQL Full-Text Search vs. Dedicated Search Service

## Status: Proposed

## Context

Baytic users need to search:
- **Clinical resources** (guidelines, toolkits) by title, keywords, specialty, species.
- **Case library** by condition, specialty, patient type, author.
- **Publications** by title, topic, author, date.
- **Directory** (clinics, specialists) by name, location, specialty.
- **Discussion posts** by thread title, content, contributor.

Performance requirements:
- MVP (0–1K users): <100ms search response time, < 1K queries/day.
- Growth (1K–100K users): <100ms response time, ~10–100K queries/day.
- Scale (100K+ users): <50ms response time, ~1M+ queries/day, relevance tuning.

Two strategies:
1. **PostgreSQL full-text search (GIN indexes)**: Start simple, built into database.
2. **Dedicated search service (OpenSearch/Elasticsearch)**: Purpose-built search engine.

## Decision

**Implement a two-phase search strategy:**

**Phase 1 (MVP, 0–16 weeks)**: Use PostgreSQL GIN indexes for full-text search.
- Query: `SELECT * FROM resources WHERE search_vector @@ plainto_tsquery('english', 'bone fracture') LIMIT 20;`
- Index size: ~1–2 GB (manageable for MVP dataset).
- Query latency: 10–50ms for typical searches.
- Operational overhead: Zero (built into PostgreSQL).

**Phase 2 (Growth, Week 16–32)**: Migrate to OpenSearch cluster if search becomes a bottleneck.
- Trigger: Query latency exceeds 100ms OR index size exceeds 50GB OR QPS exceeds 500 reads/sec.
- Implementation: Parallel indexing to both PostgreSQL and OpenSearch; switch read traffic gradually.
- Benefits: Advanced relevance tuning, faceted search, autocomplete, analytics.

## Rationale

### Why PostgreSQL GIN for MVP?

1. **Zero operational overhead**: No additional services to run, monitor, or scale. PostgreSQL manages indexing automatically.

2. **Fast enough**: GIN indexes are competitive with Elasticsearch for up to ~100GB of data. MVP dataset (clinical guidelines, cases, publications) is likely <10GB.

3. **Strong consistency**: Updates are immediately queryable (no lag like eventual-consistency search services). Critical for approving/unpublishing content.

4. **Cost-effective**: No additional infrastructure costs. Managed PostgreSQL handles backups and replicas.

5. **Built-in RBAC**: Row-level security policies can restrict search results per user role without complex application logic.

6. **Language support**: Built-in stemming for English, French, German, Russian, and 9+ other languages. Sufficient for MVP.

7. **Ranking**: Relevance ranking with `ts_rank()` and `ts_rank_cd()` functions. Comparable to Elasticsearch at MVP scale.

### Why NOT Elasticsearch/OpenSearch for MVP?

1. **Operational complexity**: Requires dedicated cluster management, shard rebalancing, and monitoring. Adds 1–2 hours per week ops overhead.

2. **Cost**: Managed service (e.g., AWS OpenSearch) costs $200–500/month minimum. Self-hosted requires infrastructure and expertise.

3. **Eventual consistency**: Index updates lag behind PostgreSQL writes by seconds. Can cause confusion (user publishes resource, doesn't appear in search for 5 sec).

4. **Over-engineered**: Elasticsearch shines at 100GB+ datasets and 1000+ QPS. MVP is an order of magnitude smaller.

5. **Integration complexity**: Requires dual-write logic (update both PostgreSQL and Elasticsearch), change data capture (CDC) pipeline, or async indexing. Introduces bugs and latency.

6. **Licensing concerns**: Elasticsearch moved to Elastic License (not open-source). OpenSearch is open-source but less feature-rich.

## Alternatives Considered

### Option A: Algolia (Managed Search Service)

**Pros**: Excellent relevance out-of-the-box, fast, fully managed, great for autocomplete.

**Rejection**:
- Per-query pricing (~$0.01 per search) adds up at scale. 10K queries/day = $100/month minimum.
- Third-party dependency for core feature.
- Overkill for MVP; PostgreSQL is sufficient.

### Option B: Meilisearch (Self-Hosted)

**Pros**: Simple Rust-based search engine, good UX for end users.

**Rejection**:
- Requires self-hosting or managed service (additional cost/ops).
- Smaller community and fewer advanced features than Elasticsearch.
- Not worth the operational burden for MVP.

### Option C: No full-text search (filter by fields only)

**Pros**: Simplest to implement.

**Rejection**:
- Poor UX; users can't search across multiple fields.
- Defeats core Baytic value prop (discoverable clinical knowledge).

### Option D: Elasticsearch/OpenSearch from day 1

**Pros**: Unlimited scale from the start, advanced relevance tuning.

**Rejection**:
- Premature optimization. Infrastructure cost and operational complexity are not justified until search is a bottleneck.
- Time cost: 1–2 weeks to set up, manage, and integrate. MVP timeline is tight.

## Consequences

### Positive
- **Low operational overhead**: PostgreSQL handles everything; no additional services to monitor.
- **Strong consistency**: Search results always match current data.
- **Fast query times**: 10–50ms for typical MVP workloads.
- **Cost-effective**: No additional infrastructure costs; included in managed PostgreSQL service.
- **Integrated RBAC**: Row-level security works seamlessly with search queries.
- **Future migration**: Easy path to OpenSearch when needed (extract as separate service).

### Negative
- **Scalability ceiling**: GIN indexes degrade at >100GB data or >1000 QPS.
- **Limited relevance tuning**: PostgreSQL ranking is basic; can't fine-tune like Elasticsearch.
- **No advanced features**: No faceted search, autocomplete suggestions, or analytics out-of-the-box.
- **Language limitations**: Built-in stemmers for ~15 languages; custom dictionaries require configuration.
- **Index rebuild required**: Schema changes (adding/removing columns) require manual index maintenance.

## Implementation Notes

### PostgreSQL Full-Text Search Setup

**Step 1: Enable extension** (if not already enabled)
```sql
CREATE EXTENSION IF NOT EXISTS pg_trgm;  -- For trigram indexing (alternative to GIN)
```

**Step 2: Create search vector column**
```sql
ALTER TABLE content.resources
ADD COLUMN search_vector tsvector
GENERATED ALWAYS AS (
  setweight(to_tsvector('english', title), 'A') ||
  setweight(to_tsvector('english', body), 'B') ||
  setweight(to_tsvector('english', coalesce(tags::text, '')), 'C')
) STORED;

-- Create GIN index
CREATE INDEX idx_resources_search ON content.resources USING GIN(search_vector);
```

**Step 3: Query with ranking**
```sql
SELECT 
  id, title, 
  ts_rank(search_vector, query) as rank
FROM content.resources, 
  plainto_tsquery('english', 'bone fracture') AS query
WHERE search_vector @@ query
ORDER BY rank DESC
LIMIT 20;
```

**Step 4: Add filters**
```sql
SELECT * FROM content.resources
WHERE search_vector @@ plainto_tsquery('english', 'fracture')
  AND specialty = 'orthopedics'
  AND status = 'published'
ORDER BY ts_rank(search_vector, query) DESC;
```

### Performance Tuning

1. **Index bloat monitoring**: PostgreSQL index size grows with updates. Monitor with:
   ```sql
   SELECT schemaname, tablename, 
          pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) as size
   FROM pg_tables
   ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
   ```

2. **Query optimization**: Use `EXPLAIN ANALYZE` to check query plans:
   ```sql
   EXPLAIN ANALYZE 
   SELECT * FROM resources 
   WHERE search_vector @@ plainto_tsquery('english', 'cancer');
   ```

3. **Connection pooling**: Use PgBouncer to manage connections (4 per CPU core).

### Migration to OpenSearch (Growth Phase)

**Trigger**: When any of these thresholds is hit:
- Index size >50 GB
- Query latency >100ms consistently
- QPS >500 reads/sec

**Migration steps**:
1. Set up parallel OpenSearch cluster.
2. Implement dual-write: on resource publish, write to both PostgreSQL and OpenSearch.
3. Backfill existing resources from PostgreSQL to OpenSearch (one-time operation).
4. Gradually shift read traffic to OpenSearch (A/B test 10% → 50% → 100%).
5. Monitor latency; if OpenSearch is slower, revert to PostgreSQL.
6. Once stable, deprecate PostgreSQL full-text search (keep PostgreSQL for transactional data).

**Note**: This is a deliberate delay of OpenSearch investment. Most projects over-estimate search complexity early on.

## Related Decisions

- [ADR-002: Database Design](./ADR-002-database-design.md)
- [ADR-001: Modular Monolith Architecture](./ADR-001-modular-monolith.md)

## Success Criteria

- ✅ Resource search returns results in <100ms for typical queries.
- ✅ Search results are consistent with database state (no stale data).
- ✅ Relevance ranking favors title matches over body matches.
- ✅ Search is case-insensitive and handles stemming (e.g., "fractures" matches "fracture").
- ✅ Filters by specialty, status, and date work correctly.
- ✅ RBAC policies prevent unauthorized content from appearing in search results.
- ✅ Index size remains <10 GB during MVP phase.
- ✅ No query latency degradation observed as dataset grows to 1GB.

## Long-Term Scalability

**Week 32+** (if search becomes bottleneck):
- Migrate to OpenSearch for advanced relevance and analytics.
- Implement faceted search (e.g., "7 resources by orthopedic specialists").
- Add autocomplete suggestions and trending searches.
- Consider search analytics (which queries have no results, which resources are most viewed).
