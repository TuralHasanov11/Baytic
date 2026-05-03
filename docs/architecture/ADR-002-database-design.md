# ADR-002: PostgreSQL Relational Database Design with Full-Text Search

## Status: Proposed

## Context

Baytic stores diverse data: user accounts, roles, member profiles, clinical resources, case library, publications, events, discussion posts, audit logs, and directory listings. The MVP phase (12–16 weeks) must support 0–1K users with fast, reliable access to search and discovery features.

Key requirements:
- Support RBAC (role-based access control) at the database layer for security.
- Enable full-text search across resources, cases, publications, and directory without external dependencies.
- Track content versioning and audit trail for compliance (GDPR, data deletion).
- Scale from MVP (single instance) to Growth phase (read replicas).
- Maintain data consistency for critical paths (auth, member data, CE tracking).

Considered databases: PostgreSQL.

## Decision

**Adopt PostgreSQL 14+ as the primary relational database for Baytic MVP and beyond.**

Design approach:
1. Single normalized schema organized by domain module (profiles, content, cases, events, community, directory).
2. Logical schema separation via namespace/prefix (e.g., `content_*` table names).
3. Full-text search using PostgreSQL GIN indexes on searchable columns (MVP); migrate to dedicated OpenSearch service at growth phase.
4. Prepared statements and parameterized queries for all client code (.NET/EF Core).

## Rationale

1. **Full-text search built-in**: PostgreSQL provides GIN indexes and query operators (`@@`, `<->`, `||`) for relevance-ranked full-text search without external services. Sufficient for MVP; easy to migrate to OpenSearch later.

2. **RBAC and row-level security (RLS)**: PostgreSQL row-level security policies enable fine-grained access control at the database layer, protecting sensitive data even if application logic has bugs.

3. **Mature ecosystem**: Well-tested for relational data, strong ORM support (Entity Framework Core), extensive documentation, large talent pool.

4. **Cost-effective**: Managed PostgreSQL services (Azure Database for PostgreSQL, AWS RDS) are cheaper than comparable NoSQL solutions at MVP scale. Reserved capacity available for growth phase.

5. **ACID transactions**: Ensures data consistency for critical paths (publish resource).

6. **Extensibility**: JSON columns for flexible metadata; custom extensions (UUID, PostGIS) if needed later.

7. **Audit and compliance**: JSON logging, immutable audit tables, and timestamped records support GDPR data access/deletion workflows and compliance reviews.

## Consequences

### Positive
- **Search performance**: GIN indexes provide millisecond full-text search queries without external services.
- **Data integrity**: ACID transactions prevent corrupted data (e.g., member signup failure mid-transaction).
- **Audit compliance**: Immutable audit logs + RLS make compliance reviews straightforward.
- **Cost efficient**: Managed PostgreSQL service is cheaper than managing multiple databases.
- **ORM flexibility**: Both Node.js (Prisma) and .NET (EF Core) have excellent PostgreSQL support.
- **Future migration path**: Clear path to OpenSearch (MVP) → multi-region replicas (Growth) → sharding (Scale) without rewriting business logic.

### Negative
- **Vertical scaling limit**: Single instance cannot scale writes beyond ~5K concurrent connections. Must add read replicas or extract services for write-heavy workloads.
- **Schema migration risk**: Schema changes on large tables (e.g., `resources`) require careful planning (e.g., online migration tools, downtime windows).
- **Full-text search limitations at scale**: GIN indexes work well up to ~100GB. Beyond that, dedicated search service is required for relevance tuning and speed.
- **Denormalization burden**: Application must handle some denormalization (e.g., caching user role in token instead of querying every time) to avoid N+1 queries.

## Implementation Notes

### MVP Database Schema Organization

```sql
-- Auth domain
-- Managed by Identity Provider (Keycloak)

-- Content domain
CREATE TABLE content.resources (
  id UUID PRIMARY KEY,
  title VARCHAR(500) NOT NULL,
  body TEXT NOT NULL,
  status VARCHAR(50) NOT NULL DEFAULT 'draft', -- draft, submitted, approved, published
  version INT NOT NULL DEFAULT 1,
  created_by UUID REFERENCES auth.users(id),
  published_by UUID REFERENCES auth.users(id),
  published_at TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL,
  updated_at TIMESTAMPTZ NOT NULL,
  
  -- Full-text search
  search_vector tsvector GENERATED ALWAYS AS (
    to_tsvector('english', title) || to_tsvector('english', body)
  ) STORED
);

CREATE INDEX idx_resources_search ON content.resources USING GIN(search_vector);

```

### Query Optimization

- **Prepared statements**: Use parameterized queries to prevent SQL injection and enable query plan caching.
- **Indexes**: Create indexes on frequently filtered/joined columns (user roles, resource status, timestamps).
- **Connection pooling**: Use PgBouncer (4 connections per CPU core) to avoid connection exhaustion.
- **Monitoring**: Enable `pg_stat_statements` to identify slow queries; monitor query latency with observability stack.

### Migration to OpenSearch (Growth Phase)

When full-text search becomes a bottleneck (index >100GB, QPS >500 reads/sec):
1. Run parallel OpenSearch cluster.
2. Implement dual-write pattern: write to both PostgreSQL and OpenSearch.
3. Backfill OpenSearch with existing data.
4. Switch read traffic to OpenSearch; maintain PostgreSQL as source of truth.
5. Deprecate PostgreSQL full-text search.

## Related Decisions

- [ADR-001: Modular Monolith Architecture](./ADR-001-modular-monolith.md)
- [ADR-004: Search Service Strategy](#) (TBD)

## Migration Path (Future)

If single PostgreSQL instance becomes a bottleneck (Phase B - Growth):
1. Add read replicas for read-heavy queries.
2. Implement connection pooling (PgBouncer).
3. Denormalize frequently accessed data (cache in application layer).
4. Migrate audit logs to cold storage (archive tables after 7 years).
5. Extract specialized services (search, analytics) into separate databases if needed.

## Success Criteria

- ✅ Schema is normalized and organized by domain.
- ✅ Full-text search works with single-digit millisecond latency on MVP dataset.
- ✅ Row-level security policies are implemented for member-only content.
- ✅ Data consistency tests pass (ACID transactions, foreign key constraints).
- ✅ Monitoring alerts fire if query latency exceeds 100ms or connection count exceeds 80% capacity.
