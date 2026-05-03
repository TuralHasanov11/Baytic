# ADR-005: API Design Patterns & Versioning Strategy

## Status: Proposed

## Context

Baytic APIs serve:
- **Frontend application** (React/Vue + Next.js/Nuxt) with web and mobile-responsive UX.
- **Future third-party integrations** (clinic management systems, scheduling tools, analytics platforms).
- **Potential mobile apps** (iOS, Android) in future phases.

Requirements:
- Clear, intuitive API contracts for frontend team and future partners.
- Graceful evolution: add features without breaking existing clients.
- Consistent error handling and response format.
- Rate limiting and pagination for large result sets.
- Support for filtering, sorting, and searching.

Key challenges:
- Membership tiers have different access levels; responses must respect RBAC.
- Content is versioned (resources have multiple revisions); API must distinguish published vs. draft.
- Member profiles are privacy-sensitive; different roles see different information.
- Async operations (publishing, indexing) need to reflect in real-time search.

Two design approaches:
1. **REST with URL versioning** (`/api/v1/resources`, `/api/v2/resources`): Clearer evolution, easier for multiple clients.
2. **REST with header versioning** (`Accept: application/vnd.baytic.v1+json`): Cleaner URLs, requires client sophistication.
3. **GraphQL**: Maximum flexibility; potentially over-engineered for MVP.

## Decision

**Adopt REST API with URL versioning and a consistent response envelope.**

Base URL: `https://api.baytic.com/api/v1/`

### Response Format

All responses (success and error) use a consistent envelope:

**Success (200, 201, etc.)**:
```json
{
  "data": { "id": "...", "title": "..." },
  "meta": {
    "timestamp": "2026-05-03T10:30:00Z",
    "version": "1.0",
    "page": 1,
    "pageSize": 20,
    "total": 150
  }
}
```

**Error (4xx, 5xx)**:
```json
{
  "error": {
    "code": "RESOURCE_NOT_FOUND",
    "message": "Resource with ID xyz not found",
    "details": {
      "resourceId": "xyz",
      "suggestion": "Check the resource ID and try again"
    },
    "timestamp": "2026-05-03T10:30:00Z"
  }
}
```

### Endpoint Patterns

**List** (with pagination and filters):
```
GET /api/v1/resources?page=1&pageSize=20&specialty=orthopedics&status=published
Response: 200 OK
{
  "data": [
    { "id": "...", "title": "Bone Fracture Care", "specialty": "orthopedics", "status": "published" },
    ...
  ],
  "meta": { "page": 1, "pageSize": 20, "total": 250 }
}
```

**Get by ID**:
```
GET /api/v1/resources/{id}
Response: 200 OK (or 404 Not Found)
```

**Create**:
```
POST /api/v1/resources
Body: { "title": "...", "body": "...", "specialty": "..." }
Response: 201 Created
Location: /api/v1/resources/{newId}
{
  "data": { "id": "{newId}", "title": "...", "status": "draft" }
}
```

**Update**:
```
PUT /api/v1/resources/{id}
Body: { "title": "New Title", ... }
Response: 200 OK
```

**Delete**:
```
DELETE /api/v1/resources/{id}
Response: 204 No Content (or 200 OK with empty body)
```

**Async Operations** (e.g., publish resource):
```
POST /api/v1/resources/{id}/publish
Response: 202 Accepted
Location: /api/v1/resources/{id}/publish-status
{
  "data": {
    "operationId": "op-123",
    "status": "pending",
    "resourceId": "{id}"
  }
}

-- Check status later:
GET /api/v1/resources/{id}/publish-status?operationId=op-123
Response: 200 OK
{
  "data": {
    "operationId": "op-123",
    "status": "completed",
    "result": { "resourceId": "{id}", "publishedAt": "2026-05-03T10:30:00Z" }
  }
}
```

## Rationale

1. **REST over GraphQL for MVP**: REST is simpler to version, cache, and understand for small teams. GraphQL is powerful but adds complexity (schema stitching, resolver optimization). Revisit at scale.

2. **URL versioning over header versioning**:
   - URL versioning (`/api/v1`, `/api/v2`) is explicit and easier for frontend devs to understand.
   - Header versioning is cleaner aesthetically but requires clients to know about versioning (adds friction).
   - Hybrid approach: support both (URL is primary, header is secondary for advanced use cases).

3. **Consistent response envelope**:
   - Single data key for payloads; single error key for errors (no mixing).
   - Metadata (timestamp, version, pagination) always present, reduces parsing logic.
   - Easier to build client libraries and documentation.

4. **Pagination required**: Prevents API abuse (e.g., fetching 1M resources in one request). Default pageSize=20, max=100.

5. **Clear error codes**: Named error codes (not just HTTP status codes) help clients handle specific cases (e.g., `ROLE_INSUFFICIENT` vs. `UNAUTHORIZED`).

6. **Location header for POST**: Tells client where the new resource is; enables automatic navigation in SPAs.

7. **202 Accepted for async work**: Distinguishes long-running operations from instant responses. Clients poll status endpoint.

## Alternatives Considered

### Option A: GraphQL

**Pros**: Clients request exactly the fields they need; reduces over-fetching.

**Rejection**:
- Over-engineered for MVP with single frontend app.
- Requires GraphQL expertise on team (learning curve).
- Harder to cache than REST (POST-based, custom caching logic).
- Pagination and filtering are trickier (GraphQL pagination is cursor-based by default).
- Better to start with REST; add GraphQL layer later if multiple clients exist.

### Option B: Header versioning only

**Pros**: Cleaner URLs; clients can upgrade to new API without changing URL.

**Rejection**:
- Less discoverable; developers don't know API versioning exists if only in headers.
- Harder to deprecate old versions (how do you force clients off v1?).
- Hybrid approach is better: URL version is primary; headers for power users.

### Option C: Resource-specific versioning (e.g., `/resources/v2` vs. `/cases/v1`)

**Pros**: Fine-grained control per resource type.

**Rejection**:
- Confusing; clients must track multiple API versions simultaneously.
- Harder to maintain (duplicate code in old/new versions).
- Global versioning is simpler.

### Option D: Hypermedia API (HATEOAS)

**Pros**: Clients discover endpoints dynamically; reduces coupling.

**Rejection**:
- Over-engineered for MVP; adds complexity without clear benefit.
- SPAs have hardcoded API URLs anyway; HATEOAS doesn't reduce coupling much.

## Consequences

### Positive
- **Clear contracts**: Frontend and backend teams share explicit API contracts; reduces integration bugs.
- **Versioning safety**: Adding a new API version doesn't break old clients. Gradual migration path.
- **Consistent error handling**: All errors follow the same structure; frontend can build generic error handler.
- **Scalable**: Pagination prevents memory bloat. Rate limiting can be applied per endpoint.
- **Future integration**: Third parties can build on top of APIs without surprises.
- **Discoverability**: API versioning is explicit in URLs; easy to document and communicate.

### Negative
- **URL bloat**: Every breaking change requires new version (`/api/v1`, `/api/v2`, etc.). Old versions must be maintained or deprecated.
- **Deprecation pain**: Maintaining multiple API versions increases operational burden (bug fixes must be backported).
- **Verbosity**: Response envelope adds ~100 bytes per response (negligible impact but "noisier" than minimal JSON).
- **Over-engineering risk**: Teams often version too frequently. Discipline is required to avoid API sprawl.

## Implementation Notes

### Endpoint Taxonomy

**Resources** (content guidelines, toolkits):
```
GET    /api/v1/resources
GET    /api/v1/resources/{id}
GET    /api/v1/resources/{id}/versions
POST   /api/v1/resources
PUT    /api/v1/resources/{id}
DELETE /api/v1/resources/{id}
POST   /api/v1/resources/{id}/publish
POST   /api/v1/resources/{id}/unpublish
```

**Cases** (case library):
```
GET    /api/v1/cases
GET    /api/v1/cases/{id}
POST   /api/v1/cases
PUT    /api/v1/cases/{id}
DELETE /api/v1/cases/{id}
```

**Members** (user profiles):
```
GET    /api/v1/members/me
PUT    /api/v1/members/me
GET    /api/v1/members/{id}
GET    /api/v1/members/{id}/public-profile
```

**Search** (cross-domain search):
```
GET    /api/v1/search?q=query&domains=resources,cases,directory
```

**Discussion** (community posts):
```
GET    /api/v1/discussions
GET    /api/v1/discussions/{id}
POST   /api/v1/discussions
POST   /api/v1/discussions/{id}/replies
DELETE /api/v1/discussions/{id}
```

### Rate Limiting

Implement per-role rate limits:
- Public: 10 req/min
- Student: 100 req/min
- Veterinarian: 500 req/min
- Admin: unlimited

Response headers:
```
X-RateLimit-Limit: 500
X-RateLimit-Remaining: 499
X-RateLimit-Reset: 1234567890
```

429 Too Many Requests on limit exceeded.

### Pagination

Standard query parameters:
```
?page=1&pageSize=20&sortBy=createdAt&sortOrder=desc
```

Constraints:
- Max pageSize: 100
- Default pageSize: 20
- Max offset: 100,000 (prevents deep pagination abuse)

Response metadata:
```json
"meta": {
  "page": 2,
  "pageSize": 20,
  "total": 250,
  "hasNextPage": true,
  "hasPreviousPage": true
}
```

### Versioning Strategy

**Version lifecycle**:
1. **v1** (stable): Existing clients using this version.
2. **v2** (beta): New version, parallel with v1. Documented, but not recommended yet.
3. **v1** (deprecated): Announce 6-month deprecation period.
4. **v1** (sunset): Stop accepting requests; return 410 Gone.

**When to version**:
- Remove an endpoint: new version.
- Remove a required request field: new version.
- Remove a response field: new version (breaking for clients parsing JSON).
- Add optional request field: no new version.
- Add optional response field: no new version (clients ignore unknown fields).
- Change error code: new version.

### Documentation

Use OpenAPI 3.0 specification. Example:
```yaml
openapi: 3.0.0
info:
  title: Baytic API
  version: 1.0.0
paths:
  /resources:
    get:
      operationId: listResources
      parameters:
        - name: page
          in: query
          schema:
            type: integer
            default: 1
      responses:
        '200':
          description: List of resources
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ResourceListResponse'
```

### Error Codes

Standardized error codes:
```
VALIDATION_ERROR         : Request validation failed
AUTHENTICATION_REQUIRED  : No credentials provided
ROLE_INSUFFICIENT       : User lacks required role
RESOURCE_NOT_FOUND      : Resource does not exist
CONFLICT                : Resource already exists or constraint violated
RATE_LIMIT_EXCEEDED     : Too many requests
SERVER_ERROR            : Internal server error
SERVICE_UNAVAILABLE     : Dependency (e.g., email service) is down
```

## Related Decisions

- [ADR-002: Database Design](./ADR-002-database-design.md) (impacts query design)
- [ADR-003: Authentication](./ADR-003-authentication-strategy.md) (impacts RBAC enforcement)

## Success Criteria

- ✅ All endpoints return consistent response envelope (data + meta or error + meta).
- ✅ Pagination works correctly; max pageSize is 100.
- ✅ Rate limiting enforces per-role limits.
- ✅ API documentation is generated from OpenAPI spec and kept in sync.
- ✅ Version deprecation is communicated 6 months in advance.
- ✅ Old API versions can be sunset without breaking compliant clients.
- ✅ Frontend team can integrate without API surprises.
- ✅ Error responses include actionable error codes and suggestions.

## Evolution (Future)

If multiple external clients emerge:
1. Publish OpenAPI spec for partners.
2. Provide generated SDKs (TypeScript, Python, C#) for common languages.
3. Add GraphQL layer alongside REST (not replacement).
4. Consider API gateway for rate limiting, versioning, and observability (e.g., Kong, Apigee).
