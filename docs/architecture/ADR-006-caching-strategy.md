# ADR-006: Caching Strategy - Multi-Layer Cache for MVP Scalability

## Status: Proposed

## Context

Baytic serves both read-heavy (searching resources, browsing directory) and write-heavy (publishing content, updating CE credits) workloads:

**Read-heavy paths**:
- Veterinarians search guidelines (100s per second at scale).
- Members browse case library (10s per second).
- Directory lookups by location/specialty (10s per second).
- Public visitors read announcements and public resources.

**Write paths**:
- Admins publish resources (low frequency, ~10 per day).
- Members update CE credits (medium frequency, ~100 per day during events).
- Discussion posts (low frequency, ~10 per day).

Caching opportunities:
- **Object cache** (Redis): Frequently accessed data (resources, cases, profiles).
- **HTTP cache**: Browser cache (static assets) + CDN (global edge nodes).
- **Query result cache**: Database query results.
- **Computed cache**: Expensive calculations (e.g., member dashboard statistics).

Performance goals:
- MVP: <500ms response time for most endpoints.
- Growth: <200ms response time.
- Scale: <100ms response time (tail latency <1s).

## Decision

**Implement a three-tier caching strategy:**

### Tier 1: Browser & CDN Cache (HTTP)
- **What**: Static assets (CSS, JavaScript, images), public resources, public announcements.
- **Duration**: 
  - Immutable assets (versioned, e.g., `/assets/app-abc123.js`): 1 year.
  - Public resources (guidelines, published cases): 5 minutes to 1 hour (configurable).
  - Public announcements: 5 minutes.
- **Implementation**: Cache-Control headers set by backend; CDN (Azure CDN or CloudFlare) caches at edge.

### Tier 2: Object Cache (Redis)
- **What**: Frequently accessed data (member profiles, resource metadata, case summaries).
- **Duration**: 1–30 minutes depending on data freshness requirements.
- **Invalidation**: Explicit (delete key on write) + TTL expiry.
- **Implementation**: Redis cluster (managed service for MVP) or single Redis instance.

### Tier 3: Database Query Cache
- **What**: Paginated list results, filtered searches (to be determined).
- **Duration**: 5–15 minutes (searches are relatively static).
- **Invalidation**: On any resource publish/update.
- **Implementation**: Query result caching via application layer (e.g., Prisma query cache or custom).

## Rationale

1. **Reduces database load**: Most traffic is read traffic. Caching answers 80% of requests without hitting database.

2. **Improves latency**: Cache hits respond in <50ms (Redis) vs. >100ms (database query + processing).

3. **Scales cost-effectively**: One Redis instance can handle 100K requests/sec. Much cheaper than scaling database.

4. **Flexibility**: Three-tier approach allows fine-grained control over freshness vs. latency trade-off.

5. **Familiar tooling**: Redis is industry-standard; all team members know how to use it.

6. **Stateless API**: Caching in Redis, not in-memory on app servers, allows horizontal scaling of API nodes.

## Alternatives Considered

### Option A: No caching; rely on database optimization

**Pros**: Simpler architecture; no cache invalidation bugs.

**Rejection**:
- Database can only scale vertically (bigger machine) until single-machine limits are hit.
- Horizontal scaling requires read replicas + complex failover logic.
- Higher costs than caching.

### Option B: Cache everything in application memory

**Pros**: Fastest (no network round trip to Redis).

**Rejection**:
- Not sharable across API instances (if running multiple containers).
- Hard to invalidate (must evict from all instances).
- Difficult to reason about cache consistency.

### Option C: Use a distributed cache (e.g., Memcached) alongside database

**Pros**: Lightweight, battle-tested for caching.

**Rejection**:
- Redis is more feature-rich (data structures, Lua scripting, persistence).
- Memcached doesn't offer persistence; data is lost on restart.
- Similar performance for MVP workloads.

### Option D: Full-page caching with cache busting (e.g., Varnish)

**Pros**: Maximum performance for static content.

**Rejection**:
- Overkill for MVP; adds operational complexity.
- CDN + browser cache + object cache covers most use cases.
- Revisit at scale if needed.

## Consequences

### Positive
- **Dramatically lower latency**: Cached responses serve in <50ms; uncached in 100–500ms.
- **Reduced database load**: 80% of requests hit cache; database handles 20% (writes + cache misses).
- **Horizontal scaling**: Stateless API servers + external cache enable easy scaling.
- **Cost savings**: One Redis instance is cheaper than scaling database replicas.
- **User experience**: Faster page loads, snappier search results.

### Negative
- **Cache invalidation complexity**: Invalidating at wrong time causes stale data; not invalidating wastes cache.
- **Cache misses**: New or rarely-accessed data experiences latency spike on first load.
- **Memory overhead**: Redis requires dedicated memory budget. Monitor to avoid eviction.
- **Operational overhead**: One more service to monitor and operate (alerting on Redis memory, connection count).
- **Stale data risk**: If cache invalidation fails, users see outdated information (mitigated with TTL).

## Implementation Notes

### Tier 1: HTTP Cache Headers

Set at API response level:

**For public, immutable resources** (published guidelines, cases):
```
Cache-Control: public, max-age=3600, immutable
ETag: "abc123def456"
CDN-Cache-Control: max-age=86400  (CDN caches for 1 day)
```

**For member-only content** (requires auth):
```
Cache-Control: private, max-age=60
Vary: Authorization
```

**For real-time data** (search results, notifications):
```
Cache-Control: no-cache, private
```

### Tier 2: Redis Object Cache

**Setup**: Single Redis instance (managed service) or cluster for high availability.

**Usage pattern** (pseudocode):

```csharp
// Get resource by ID
public async Task<Resource> GetResourceAsync(string id)
{
    var cacheKey = $"resource:{id}";
    
    // Try cache first
    var cached = await _cache.GetAsync<Resource>(cacheKey);
    if (cached != null) return cached;
    
    // Cache miss; query database
    var resource = await _db.Resources.FindAsync(id);
    if (resource == null) return null;
    
    // Store in cache for 10 minutes
    await _cache.SetAsync(cacheKey, resource, TimeSpan.FromMinutes(10));
    
    return resource;
}

// Invalidate on write
public async Task PublishResourceAsync(string id)
{
    var resource = await _db.Resources.FindAsync(id);
    resource.Status = "published";
    resource.PublishedAt = DateTime.UtcNow;
    
    await _db.SaveChangesAsync();
    
    // Invalidate cache
    await _cache.DeleteAsync($"resource:{id}");
    await _cache.DeleteAsync("resources:list:*");  // Invalidate list caches
}
```

**Cache keys** (naming convention):
```
resource:{id}
resources:list:published:specialty={specialty}:page={page}
resource:{id}:versions
member:{id}:profile
member:{id}:ce-credits
case:{id}
search:query={q}:domain={domain}:page={page}
```

**Cache invalidation strategy**:
- Explicit: On create/update/delete, delete related cache keys.
- TTL: Set reasonable TTL (1–30 minutes) to auto-expire.
- Broadcast: If multiple services exist, publish cache invalidation to message queue.

### Tier 3: Query Result Cache

For list/search endpoints, cache results:

```csharp
public async Task<PaginatedList<Resource>> SearchResourcesAsync(string query, int page, int pageSize)
{
    var cacheKey = $"search:{query}:page={page}:pageSize={pageSize}";
    
    var cached = await _cache.GetAsync<PaginatedList<Resource>>(cacheKey);
    if (cached != null) return cached;
    
    // Cache miss; execute search
    var results = await _db.Resources
        .Where(r => r.SearchVector @@ _query)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    var paginatedResults = new PaginatedList<Resource>(results, total, page, pageSize);
    
    await _cache.SetAsync(cacheKey, paginatedResults, TimeSpan.FromMinutes(5));
    
    return paginatedResults;
}
```

### Cache Eviction Policy

Set Redis eviction policy (if memory limit is hit):
```
# Prefer LRU (least recently used)
maxmemory-policy allkeys-lru
maxmemory 4gb
```

Monitor cache hit rate; target >80% hit rate.

### Monitoring

Alert on:
- **Redis memory usage** >90% (approaching eviction).
- **Cache hit rate** <70% (too many misses).
- **Redis latency** >100ms (slow cache access).
- **Redis connection count** >1000 (potential connection leak).

### Testing

Test cache behavior:

```csharp
[Fact]
public async Task GetResource_FirstCall_FetchesFromDatabase()
{
    // First call
    var resource1 = await _service.GetResourceAsync("r123");
    
    // Verify database was hit
    _mockDb.Verify(x => x.Resources.FindAsync("r123"), Times.Once);
}

[Fact]
public async Task GetResource_SecondCall_ReturnsCached()
{
    // First call
    await _service.GetResourceAsync("r123");
    
    // Second call
    var resource2 = await _service.GetResourceAsync("r123");
    
    // Verify database was only hit once
    _mockDb.Verify(x => x.Resources.FindAsync("r123"), Times.Once);
}

[Fact]
public async Task PublishResource_InvalidatesCache()
{
    // Cache a resource
    await _service.GetResourceAsync("r123");
    
    // Publish it
    await _service.PublishResourceAsync("r123");
    
    // Verify cache was invalidated
    var cached = await _cache.GetAsync("resource:r123");
    Assert.Null(cached);
}
```

## Related Decisions

- [ADR-002: Database Design](./ADR-002-database-design.md)
- [ADR-004: Search Strategy](./ADR-004-search-strategy.md) (caching search results)
- [ADR-005: API Design](./ADR-005-api-design.md) (HTTP cache headers)

## Transition Path

**MVP (Weeks 0–16)**: Single Redis instance, Tier 1 + Tier 2 caching.

**Growth (Weeks 16–32)**: 
- Add Redis cluster for high availability (if single instance becomes bottleneck).
- Implement Tier 3 (query result caching).
- Add cache warming for popular resources (load cache on app startup).

**Scale (Week 32+)**:
- Implement distributed cache invalidation (publish-subscribe).
- Add cache analytics (which keys are accessed most frequently).
- Consider cache-aside vs. write-through patterns based on data freshness requirements.

## Success Criteria

- ✅ Cache hit rate is ≥80% for read-heavy endpoints.
- ✅ Cached responses serve in <50ms.
- ✅ Cache invalidation is correct (stale data is not served after publish).
- ✅ Redis memory usage stays <90% of allocated memory.
- ✅ No cache leaks (memory growing unbounded over time).
- ✅ All cache keys have TTL to prevent indefinite accumulation.
- ✅ Monitoring alerts fire before cache issues impact users.
