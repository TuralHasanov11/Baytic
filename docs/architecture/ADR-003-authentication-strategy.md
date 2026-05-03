# ADR-003: Authentication Strategy - Managed Identity Provider vs. Custom

## Status: Proposed

## Context

Baytic must support:
- User registration (veterinarians, students, public) with email verification.
- Secure login/logout with session management.
- Password reset and account recovery.
- Multi-factor authentication (future, Phase 3+).
- Role-based access control across frontend and backend.
- GDPR compliance (data deletion, export).
- Integration with SSO providers (future, for clinics and enterprise).

The MVP team is 5–7 people with a 12–16 week timeline. Authentication is a critical security boundary; mistakes can expose member data or enable unauthorized access.

Two options:
1. **Managed Auth Service** (Auth0, Okta, Azure AD B2C, Keycloak): Outsource identity to a specialized provider.
2. **Custom Minimal Auth**: Build in-house with bcrypt, JWT, and email verification.

## Decision

**Adopt a managed authentication service for MVP.**

Recommended providers (in priority order):
1. **Azure AD B2C** (if using Azure infrastructure)
2. **Auth0** (if cloud-agnostic; excellent developer experience)
3. **Keycloak** (if self-hosted on-premises; open-source)

Backend integration: REST API with JWT bearer tokens; frontend integration via OAuth 2.0 with PKCE flow.

## Rationale

1. **Compliance out-of-the-box**: Managed providers handle GDPR data deletion, SOC 2 audits, and encryption. Building this in-house introduces regulatory risk.

2. **Faster delivery**: No need to build password hashing, email verification, session management, password reset, or MFA from scratch. Saves 2–3 weeks of development.

3. **Security best practices**: Managed providers employ security experts; password hashing, token expiration, and refresh token rotation are battle-tested.

4. **No database secrets**: Authentication tokens are issued by the provider, not stored in your database. Reduces attack surface (e.g., token compromise affects provider only, not your app).

5. **Scalability**: Managed services handle rate limiting, concurrent logins, and geographic redundancy. You don't need to tune auth infrastructure.

6. **Enterprise SSO**: Clinics and large organizations often require SAML/OpenID Connect for single sign-on. Managed providers support this out-of-the-box.

7. **Audit trail**: Built-in logging of login attempts, password changes, and role assignments. Supports compliance reviews.

8. **Cost-effective for MVP**: Most managed providers offer generous free/trial tiers for up to 1K–10K monthly active users. Costs scale linearly with users.

## Alternatives Considered

### Option A: Custom Auth with bcrypt + JWT

**Description**: Build minimal auth in-house: email/password signup, bcrypt hashing, JWT tokens, refresh token rotation.

**Pros**:
- Full control over auth logic and data.
- No third-party dependencies.
- Slightly faster on-request (no external API calls).

**Rejection**:
- **Security risk**: Easy to implement incorrectly (e.g., timing attacks on password comparison, insufficient PBKDF2 iterations, no rate limiting on login attempts).
- **Compliance burden**: Building audit logs, data deletion workflows, and SOC 2 compliance reports manually is error-prone.
- **Operational overhead**: Must monitor for suspicious login patterns, brute force attacks, and compromised tokens. Requires intrusion detection and incident response procedures.
- **Feature gap**: Building MFA, password complexity validation, account lockout, and session revocation adds complexity that managed services provide.
- **Team risk**: Requires security expertise on a small team; if the auth engineer leaves, knowledge is lost.
- **Time cost**: Estimated 3–4 weeks of development vs. 3–4 days for integration.

### Option B: AWS Cognito

**Pros**: Tight AWS integration, generous free tier, strong auth capabilities.

**Rejection**:
- Team uses Azure infrastructure (not AWS).
- Adds cloud vendor lock-in if team wants to migrate later.
- Less developer-friendly than Auth0; more configuration required.

### Option C: Firebase Authentication

**Pros**: Fully managed, simple integration, real-time updates.

**Rejection**:
- Firebase is Google-only; doesn't align with Azure-native team.
- Full-text search and compliance workflows require Firebase Extensions (added complexity).
- Vendor lock-in to Google Cloud.

### Option D: Build with Okta OIDC

**Description**: Use Okta for identity, but build custom authorization (roles, permissions) in your app.

**Pros**: Clean separation; Okta handles auth, app handles authz.

**Rejection**: Still requires external dependency; Okta costs scale quickly at enterprise volumes. Better to use Okta or Auth0 for both auth + basic authz, then handle complex role logic in app.

## Consequences

### Positive
- **Compliance**: GDPR-ready with one-click data deletion and export workflows.
- **Security**: Password reset, email verification, and rate limiting handled by experts.
- **Faster launch**: 3–4 days to integrate OAuth 2.0 flow vs. 3–4 weeks to build securely.
- **Future MFA**: Easy to enable multi-factor authentication when requirements grow.
- **Enterprise SSO**: Support SAML, OpenID Connect for clinics/organizations without extra work.
- **Audit trail**: Built-in logging of all auth events (logins, password changes, role assignments).
- **Team focus**: Your team focuses on business logic, not infrastructure.

### Negative
- **Third-party dependency**: If auth provider is down, your app cannot authenticate users (rare, but possible).
- **Cost at scale**: Per-active-user pricing; if you reach 100K users, costs can exceed self-hosted solution.
- **Data residency**: Some regions (e.g., China, Russia) may have limited managed auth provider availability.
- **Vendor lock-in**: Migrating away requires extracting all user data and secrets; not trivial.
- **External API calls**: Every login/token refresh requires a round trip to the provider (latency ~50–200ms). Acceptable for MVP; mitigate with token caching at growth phase.

## Implementation Notes

### Azure AD B2C Flow (Recommended for Azure-native team)

```
Frontend (Next.js):
  1. User clicks "Sign In"
  2. Redirect to Azure AD B2C login page
  3. User enters email, verifies email
  4. Redirected back to app with authorization code
  5. Frontend sends code to backend

Backend (ASP.NET Core):
  1. Exchange code for access token + ID token
  2. Verify token signature (Azure AD public key)
  3. Extract user ID, email, roles from ID token
  4. Issue session cookie or app JWT token
  5. Return to frontend

Frontend:
  1. Store session cookie (browser-managed, secure HttpOnly)
  2. Include cookie in subsequent API requests
  3. On logout, call backend to clear cookie
```

### Auth0 Flow (Recommended for cloud-agnostic team)

```
Frontend (React + Next.js):
  1. Use @auth0/nextjs-auth0 library
  2. Redirect to Auth0 login page (OAuth 2.0 with PKCE)
  3. User authenticates
  4. Auth0 calls backend callback with authorization code
  5. Backend exchanges code for access token

Backend (Node.js or .NET):
  1. Store access token in HTTP-only session
  2. Use Auth0 Management API to fetch roles for user
  3. Cache roles with 5-minute TTL

API Endpoints:
  - Protected: Check Authorization header for Bearer token or session cookie
  - Return 401 Unauthorized if token is invalid/expired
```

### Database Integration

**User table** (minimal):
```sql
CREATE TABLE auth.users (
  id VARCHAR(255) PRIMARY KEY,  -- Auth provider's user ID
  email VARCHAR(255) UNIQUE NOT NULL,
  auth_provider VARCHAR(50) NOT NULL,  -- 'azure-ad-b2c', 'auth0', 'custom'
  roles JSONB,  -- Cached roles (refreshed on login)
  last_login TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL,
  updated_at TIMESTAMPTZ NOT NULL
);
```

**Note**: Do NOT store passwords in your database. All password management is delegated to the auth provider.

### RBAC Implementation

Roles are issued by the auth provider and embedded in JWT tokens:
```json
{
  "sub": "user-id-123",
  "email": "vet@clinic.com",
  "roles": ["veterinarian", "admin"],
  "exp": 1234567890
}
```

Backend validates roles in every API call:
```csharp
[Authorize(Roles = "veterinarian,admin")]
public async Task<IActionResult> GetResourcesAsync()
{
  // Only users with veterinarian or admin role can access
}
```

### Logout Workflow

```
Frontend:
  1. Call POST /api/logout
  2. Backend clears session cookie or revokes token

Backend:
  1. If using auth provider session: revoke token
  2. Clear session cookie
  3. Return 204 No Content

Frontend:
  1. Redirect to login page
```

### GDPR Data Deletion

```
When user requests account deletion:
  1. Delete all user data from your database
  2. Call Auth0/Azure AD B2C API to revoke all tokens
  3. Call auth provider API to delete user account
  4. Send confirmation email
```

## Related Decisions

- [ADR-001: Modular Monolith Architecture](./ADR-001-modular-monolith.md)
- [ADR-007: Role-Based Access Control (RBAC)](./ADR-007-rbac-implementation.md) (TBD)

## Transition Path (Future)

If auth provider becomes a bottleneck or cost is prohibitive:
1. Export all user data and hashed passwords from provider.
2. Build custom auth service with exported user data.
3. Gradually migrate users to custom auth (on next login).
4. Decommission managed auth provider.

However, this is **not recommended** for MVP; focus on product delivery first.

## Success Criteria

- ✅ User can sign up, verify email, and log in within 1 minute.
- ✅ JWT tokens are validated on every API request.
- ✅ Password reset email is sent within 30 seconds.
- ✅ RBAC roles are enforced at the API layer.
- ✅ Audit logs show all login attempts and password changes.
- ✅ GDPR data deletion workflow completes within 5 minutes.
- ✅ Auth provider SLA is ≥99.9% uptime.
- ✅ Zero security vulnerabilities reported in managed provider.
