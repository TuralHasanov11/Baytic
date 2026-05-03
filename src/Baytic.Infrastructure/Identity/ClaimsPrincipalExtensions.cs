using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Baytic.Domain.Identity;

namespace Baytic.Infrastructure.Identity
{
    public static class ClaimsPrincipalExtensions
    {
        public const string UserIdClaimType = JwtRegisteredClaimNames.Sub;
        public const string NameClaimType = JwtRegisteredClaimNames.Name;
        private const string RoleClaimType = "roles";
        private const string GroupsClaimType = "groups";

        extension(ClaimsPrincipal principal)
        {
            public UserInfo FromClaimsPrincipal() =>
                new(new UserId(Guid.Parse(GetRequiredClaim(principal, UserIdClaimType))),
                    GetRequiredClaim(principal, NameClaimType),
                    [.. principal.FindAll(RoleClaimType).Select(c => c.Value)],
                    [.. principal.FindAll(GroupsClaimType).Select(c => c.Value)]);
            private string GetRequiredClaim(string claimType) =>
                    principal.FindFirst(claimType)?.Value ??
                    throw new InvalidOperationException(
                        $"Could not find required '{claimType}' claim.");
        }
    }
}