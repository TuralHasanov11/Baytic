# Baytic Architecture Decision Records (ADRs) - Complete Index

**Last Updated**: May 3, 2026  
**Document Version**: 1.0  
**Status**: Proposed

---

## Overview

This directory contains Architecture Decision Records (ADRs) for the Baytic veterinary platform MVP. ADRs document important architectural decisions with context, rationale, alternatives considered, and consequences.

**What is an ADR?**  
An ADR is a lightweight architectural decision document that:
- Captures why a decision was made (not just what).
- Documents alternatives and trade-offs.
- Serves as a historical record for future team members.
- Enables thoughtful review and discussion before implementation.

**Reading Guide**: Start with [ADR-001](#adr-001-modular-monolith-architecture) (overall architecture) and [ADR-002](#adr-002-database-design) (data layer). Then pick ADRs relevant to your role (backend, frontend, DevOps, QA).

---

## ADR Catalog

### ADR-001: Modular Monolith Architecture
**Status**: Accepted  
**Date**: May 3, 2026  
**Author**: Architecture Team

**Decision**: Baytic will be built as a modular monolith (not microservices) optimized for MVP.

**Key Points**:
- Single deployable service with clear internal module boundaries (auth, content, community, search, events, directory, audit).
- Enables future service extraction as the platform scales.
- Fits 12–16 week timeline and 5–7 person team.

**Read when**: Starting implementation; aligns all module responsibilities.

**Impact**: 
- ✅ Enables Phase 1 & Phase 2 delivery on schedule.
- ✅ Allows future scaling to microservices without rewrite.
- ⚠️ Requires discipline: modules must respect boundaries and not create tight coupling.

[→ Read full ADR](./ADR-001-modular-monolith.md)

---

### ADR-002: PostgreSQL Relational Database Design with Full-Text Search
**Status**: Proposed  
**Date**: May 3, 2026  
**Author**: Data Architecture Team

**Decision**: Use PostgreSQL 14+ as the primary database for all relational data, audit logs, and MVP full-text search.

**Key Points**:
- Normalized schema organized by domain module (auth, content, cases, events, community, directory, audit).
- Full-text search via PostgreSQL GIN indexes (sufficient for MVP; migrate to OpenSearch at growth phase).
- Append-only audit trail for compliance (GDPR, data deletion).
- Content versioning via history tables.

**Schema Organization**:
- `auth.*` tables: users, roles, permissions, sessions.
- `content.*` tables: resources, publications, resource_history.
- `case.*` tables: cases, case_history.
- `event.*` tables: events, announcements, registrations.
- `community.*` tables: discussion_posts, topics, moderation.
- `directory.*` tables: clinics, specialists, listings.
- `audit.*` tables: audit_logs (immutable append-only).

**Read when**: 
- Designing database schema.
- Implementing data access layer (ORM).
- Setting up full-text search indexes.

**Impact**:
- ✅ Single database reduces operational overhead (vs. polyglot persistence).
- ✅ GIN indexes provide fast search without external service.
- ✅ RBAC at database layer enhances security.
- ⚠️ Vertical scaling limit; need read replicas at growth phase.
- ⚠️ Schema migrations on large tables require careful planning.

[→ Read full ADR](./ADR-002-database-design.md)

---

### ADR-003: Authentication Strategy - Managed Identity Provider vs. Custom
**Status**: Proposed  
**Date**: May 3, 2026  
**Author**: Security Team

**Decision**: Use a managed authentication service (Azure AD B2C, Auth0, or Keycloak) instead of building custom auth.

**Key Points**:
- Delegates password hashing, email verification, session management, MFA, and GDPR compliance to provider.
- OAuth 2.0 + PKCE flow for frontend; JWT bearer tokens for backend.
- Backend validates JWT signatures and enforces RBAC based on roles in token.
- Zero password storage in Baytic database; all auth data in external provider.

**Recommended Providers** (in priority order):
1. **Azure AD B2C** (if Azure-native infrastructure).
2. **Auth0** (if cloud-agnostic; excellent DX).
3. **Keycloak** (if self-hosted; open-source).

**Read when**:
- Implementing signup/login flows.
- Integrating auth provider with frontend and backend.
- Designing session management and logout.

**Impact**:
- ✅ Compliance (GDPR, SOC 2) is simpler and faster.
- ✅ Security best practices are battle-tested by provider.
- ✅ Saves 3–4 weeks of development vs. building in-house.
- ✅ Enables enterprise SSO (SAML, OpenID Connect) for future customers.
- ⚠️ Third-party dependency: if provider is down, users can't log in (rare but possible).
- ⚠️ Per-active-user costs scale linearly; becomes expensive at 100K+ users.
- ⚠️ Vendor lock-in; migrating away requires extracting all user data.

[→ Read full ADR](./ADR-003-authentication-strategy.md)

---

### ADR-004: Search Strategy - PostgreSQL Full-Text Search vs. Dedicated Search Service
**Status**: Proposed  
**Date**: May 3, 2026  
**Author**: Search Architecture Team

**Decision**: Implement a two-phase search strategy.

**Phase 1 (MVP)**: Use PostgreSQL GIN indexes for full-text search across resources, cases, publications, and directory.

**Phase 2 (Growth, triggered at >100GB index size or >500 QPS)**: Migrate to OpenSearch.

**Key Points**:
- MVP: PostgreSQL handles search with no additional infrastructure.
- Query: `SELECT * FROM resources WHERE search_vector @@ plainto_tsquery('english', 'bone fracture') LIMIT 20;`
- Index maintenance: Automatic; no manual intervention needed.
- Relevance ranking: `ts_rank()` function for scoring.
- Estimated index size at end of MVP: <10GB (well within PostgreSQL efficiency).

**Search Across Domains**:
- Resources (guidelines, toolkits).
- Case library (peer-reviewed cases).
- Publications (whitepapers, research).
- Directory (clinics, specialists).
- Discussion posts (community threads).

**Read when**:
- Implementing search/filter endpoints.
- Tuning query performance.
- Planning scalability to growth phase.

**Impact**:
- ✅ Zero operational overhead (PostgreSQL manages indexing).
- ✅ Strong consistency: search results always match current data.
- ✅ Fast queries (<100ms) for MVP workload.
- ✅ Cost-effective: no additional services to manage.
- ⚠️ Scalability ceiling: degrades at >100GB data or >1000 QPS.
- ⚠️ Limited relevance tuning compared to Elasticsearch.

**Transition to OpenSearch** (Growth phase):
- Parallel indexing to both PostgreSQL and OpenSearch.
- Gradual traffic shift (10% → 50% → 100%).
- Rollback plan if OpenSearch latency is worse.

[→ Read full ADR](./ADR-004-search-strategy.md)

---

### ADR-005: API Design Patterns & Versioning Strategy
**Status**: Proposed  
**Date**: May 3, 2026  
**Author**: API Design Team

**Decision**: Adopt REST with URL versioning and a consistent response envelope.

**Key Design Patterns**:

**Consistent response format**:
```json
{
  "data": { /* payload */ },
  "meta": { "timestamp": "...", "version": "1.0", "page": 1, "pageSize": 20, "total": 150 }
}
```

**Error format**:
```json
{
  "error": { "code": "RESOURCE_NOT_FOUND", "message": "...", "details": {} },
  "meta": { "timestamp": "..." }
}
```

**Endpoint conventions**:
- `GET /api/v1/resources` — list (paginated, filterable).
- `GET /api/v1/resources/{id}` — get by ID.
- `POST /api/v1/resources` — create; returns 201 Created.
- `PUT /api/v1/resources/{id}` — update.
- `DELETE /api/v1/resources/{id}` — delete; returns 204 No Content.
- `POST /api/v1/resources/{id}/publish` — async operation; returns 202 Accepted.

**Versioning**:
- URL versioning: `/api/v1/`, `/api/v2/` for breaking changes.
- Support both URL and header versioning for flexibility.
- Deprecation: announce 6 months before sunsetting old versions.

**Pagination**:
- Query: `?page=1&pageSize=20&sortBy=createdAt&sortOrder=desc`.
- Default pageSize: 20; max: 100.
- Response includes `total`, `hasNextPage`, `hasPreviousPage`.

**Rate Limiting** (per role):
- Public: 10 req/min
- Student: 100 req/min
- Veterinarian: 500 req/min
- Admin: unlimited

**Read when**:
- Designing new endpoints.
- Integrating frontend with API.
- Building client libraries or SDKs.

**Impact**:
- ✅ Clear, consistent contracts reduce integration bugs.
- ✅ Versioning provides safe evolution path.
- ✅ Rate limiting prevents abuse.
- ✅ Standard format simplifies client code.
- ⚠️ URL bloat as versions accumulate (v1, v2, v3, ...).
- ⚠️ Maintenance burden: must backport bug fixes to multiple versions.
- ⚠️ Verbose response envelope adds ~100 bytes per response.

**Documentation**: OpenAPI 3.0 spec generated from code; auto-deployed to `/api/docs`.

[→ Read full ADR](./ADR-005-api-design.md)

---

### ADR-006: Caching Strategy - Multi-Layer Cache for MVP Scalability
**Status**: Proposed  
**Date**: May 3, 2026  
**Author**: Performance Team

**Decision**: Implement three-tier caching strategy.

**Tier 1: HTTP Cache** (browser + CDN)
- Immutable assets (CSS, JS, images): 1 year cache.
- Public resources: 5 minutes – 1 hour.
- Member content: 1–5 minutes.

**Tier 2: Object Cache** (Redis)
- Frequently accessed data: member profiles, resource metadata, case summaries.
- TTL: 1–30 minutes depending on freshness requirements.
- Explicit invalidation on write + TTL expiry.

**Tier 3: Query Result Cache**
- List and search results cached for 5–15 minutes.
- Invalidated when underlying data changes.

**Key Points**:
- Target cache hit rate: ≥80%.
- Redis instance (managed service) for MVP.
- Cached responses: <50ms latency vs. 100–500ms uncached.
- Reduces database load by ~80%.

**Cache Keys** (naming convention):
```
resource:{id}
resources:list:published:specialty={s}:page={p}
member:{id}:profile
search:query={q}:domain={d}:page={p}
```

**Invalidation patterns**:
- Explicit: Delete key on create/update/delete.
- TTL: Auto-expire after duration.
- Broadcast: Publish to message queue for multi-service setup.

**Read when**:
- Implementing caching in API endpoints.
- Tuning response latency.
- Planning database load reduction.

**Impact**:
- ✅ Dramatically lower latency (<50ms for cache hits).
- ✅ Reduced database load enables horizontal scaling.
- ✅ Cost-effective: one Redis instance much cheaper than database replicas.
- ⚠️ Cache invalidation complexity (risk of stale data).
- ⚠️ Cache misses cause latency spike.
- ⚠️ Operational overhead: one more service to monitor.

**Growth Phase**: Upgrade to Redis cluster for high availability; add cache warming.

**Scale Phase**: Implement distributed cache invalidation; add cache analytics.

[→ Read full ADR](./ADR-006-caching-strategy.md)

---

## Cross-References & Relationships

```
┌─────────────────────────────────────────────────────────────┐
│ ADR-001: Modular Monolith (Overall Architecture)           │
│         ├─→ ADR-002: Database Design (Data Layer)          │
│         ├─→ ADR-003: Authentication (Security)             │
│         ├─→ ADR-004: Search Strategy (Search Layer)        │
│         ├─→ ADR-005: API Design (Integration)              │
│         └─→ ADR-006: Caching Strategy (Performance)        │
└─────────────────────────────────────────────────────────────┘
```

**Dependency Graph**:
- ADR-001 is foundational; all others depend on it.
- ADR-002, 003, 004, 005, 006 are independent; each addresses a specific concern.
- ADR-002 and 004 are closely related (database search vs. dedicated service).
- ADR-005 and 006 are related (API response caching patterns).

---

## Decision Status Legend

| Status | Meaning |
|--------|---------|
| **Proposed** | Open for review and discussion; not yet finalized. |
| **Accepted** | Team agrees; ready for implementation. |
| **Deprecated** | Superseded by another decision; still valid for reference. |
| **Rejected** | Considered but not adopted; documented for historical context. |
| **Superseded** | Replaced by newer ADR; see ADR-XXX for update. |

---

## How to Use ADRs

### For Developers (Starting Implementation)

1. **Read ADR-001** to understand overall architecture.
2. **Read ADR-002** if you're working on data layer (database schema, ORM).
3. **Read ADR-003** if you're implementing auth flows.
4. **Read ADR-004** if you're implementing search/filter features.
5. **Read ADR-005** if you're designing/integrating APIs.
6. **Read ADR-006** if you're optimizing performance or adding caching.

### For Architects (Design Review)

1. Read all ADRs in order to understand the complete system.
2. Check for gaps or missing decisions.
3. Validate that decisions align with business requirements (from PRD).
4. Identify dependency chains and potential bottlenecks.

### For New Team Members (Onboarding)

1. Read ADR-001 first to understand the big picture.
2. Read ADRs for your functional area (backend, frontend, DevOps, QA).
3. Ask questions; ADRs should clarify, not confuse.

### For Future Architects (Scaling Beyond MVP)

1. Review ADR contexts and "Transition Path" sections for hints on next phase.
2. Create new ADRs when major decisions are needed (e.g., ADR-007: Microservices Decomposition).
3. Mark old ADRs as "Superseded" rather than deleting them.

---

## Updating ADRs

**When to update an ADR**:
- New context or information becomes available.
- Decision is revised (mark old version as "Superseded"; create new ADR).
- Consequences are observed that differ from predictions.

**How to update**:
1. Update the relevant ADR file.
2. Increment version in front matter (e.g., "1.0" → "1.1").
3. Add "Updated: YYYY-MM-DD" note.
4. Document what changed and why.

**When to create a new ADR**:
- Major architectural decision is needed.
- Superseding an existing ADR (e.g., "ADR-004 (v2): Search Strategy - OpenSearch for Growth Phase").

---

## Related Documentation

- [Architecture Plan](../baytic-architecture-plan.md) — High-level blueprint covering all decisions.
- [System Diagrams](../diagrams/) — Visual representations (system context, components, deployment, scalability, cost).
- [PRD](../prd.md) — Product requirements and functional scope.
- [Phase 1 & 2 Plan](../plan.md) — Implementation roadmap and workstreams.

---

## Questions & Discussion

**Having questions about a specific decision?**

1. Read the ADR thoroughly (including "Alternatives Considered" and "Consequences").
2. Check "Related Decisions" for context from other ADRs.
3. Discuss with architecture team or open a GitHub discussion.

**Proposing a new architectural decision?**

1. Check if an ADR already exists for the topic.
2. Create a new ADR file using the template: `ADR-NNN-decision-name.md`.
3. Follow the standard structure: Status, Context, Decision, Rationale, Alternatives, Consequences, Implementation Notes.
4. Submit for review before committing.

---

## Appendix: ADR Template

```markdown
# ADR-NNN: [Decision Title]

## Status: Proposed | Accepted | Rejected | Superseded | Deprecated

## Context
[Problem statement, constraints, requirements, and context for this decision.]

## Decision
[Clear statement of what decision was made.]

## Rationale
[Why this decision was chosen over alternatives.]

## Alternatives Considered
[Other options evaluated and why they were rejected.]

## Consequences
### Positive
- [Benefit 1]
- [Benefit 2]

### Negative
- [Trade-off 1]
- [Trade-off 2]

## Implementation Notes
[Practical guidance for implementing this decision.]

## Related Decisions
- [ADR-XXX: Related Decision](./ADR-XXX.md)

## Success Criteria
- ✅ [Criterion 1]
- ✅ [Criterion 2]
```

---

**Generated**: May 3, 2026  
**Architecture Decision Records Version**: 1.0  
**For questions, contact**: Architecture Team
