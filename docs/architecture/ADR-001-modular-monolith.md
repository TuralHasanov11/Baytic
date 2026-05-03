# ADR-001: Choose Modular Monolith Architecture for MVP

## Status: Accepted

## Context
Baytic is a veterinary platform targeting veterinary professionals, students, clinics, and researchers. The product roadmap includes:
- Phase 1: Core membership, portal, and navigation
- Phase 2: Resources, case library, and publications
- Planned Phase 3 and 4: Professional development, events, community, and directory

Team size is 5 to 7 with front-end, back-end, QA, product, and content roles. The MVP timeline is 12 to 16 weeks. The platform spans multiple domain areas (auth, content, search, community, events, directory) with clear domain boundaries.

## Decision
**Adopt a modular monolith architecture for the MVP.** The system will be a single deployable artifact organized into clear domain modules with explicit APIs and ownership. Each domain (membership, content, search, community, events, directory) is a module with defined contracts.

## Rationale
1. **Speed**: Simpler deployment and DevOps story for MVP. No service-to-service network latency, no distributed tracing overhead.
2. **Team fit**: 5–7 person team can manage one codebase with clear module boundaries. Easier onboarding and code review.
3. **Evolutionary path**: Monolith is NOT a dead-end. As the system scales (1K→100K+ users), individual modules can be extracted into microservices without rewriting the business logic.
4. **Cost**: Lower infrastructure overhead, fewer moving parts, easier to operate during MVP phase.
5. **Scalability hooks**: Module boundaries enable horizontal scaling later (e.g., extract search service when indexing becomes a bottleneck).

## Alternatives Considered
- **Microservices**: Rejected. Higher operational complexity, more moving parts, slower MVP delivery. Better for large teams and mature organizations.
- **Simple monolith without modules**: Rejected. Creates a "big ball of mud" that's hard to refactor or extract services from later.

## Consequences
- **Positive**:
  - Single codebase with one deployment pipeline.
  - Synchronous communication is simple and fast.
  - Easy to track business logic end-to-end.
  - Easier to debug and test.
  - Lower operational overhead.

- **Negative**:
  - Teams must enforce module boundaries through discipline and code review.
  - Scaling individual features requires scaling the entire monolith.
  - Database is shared; schema changes affect all modules (mitigation: use views, logical ownership).

## Implementation Notes
- Module organization: Define clear APIs and contracts for each domain.
- Database: Single relational database (PostgreSQL). Use schema views and logical partitions to separate concerns.
- Async work: Background worker processes (via job queue) for email, indexing, and scheduled tasks.
- API design: REST with consistent versioning strategy. RBAC at every endpoint.
- Caching: In-memory cache for sessions, frequently accessed content. Background refresh for large datasets.
- Search: Start with database full-text search. Extract to dedicated search service (OpenSearch/Elasticsearch) when index size or QPS requires.

## Migration Path (Future)
When the system outgrows monolithic scaling (100K+ users):
1. Extract the most independent module (e.g., search or directory) into its own service.
2. Maintain API contract; replace implementation.
3. Use async messaging (job queue or pub/sub) to decouple data updates.
4. Repeat for other modules as bottlenecks emerge.

## Related Decisions
- Tech stack: Node.js/React frontend; Node.js/.NET backend; PostgreSQL database.
- Deployment: Cloud-agnostic container-based approach (Docker, Kubernetes optional at scale).
