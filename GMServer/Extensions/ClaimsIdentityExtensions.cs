using GMServer.Common.Exceptions;
using System;
using System.Linq;
using System.Security.Claims;

namespace GMServer.Extensions
{
    public static class ClaimsIdentityExtensions
    {
        public static string UserID(this ClaimsPrincipal principal) => GetRequiredClaim(principal, Authentication.ClaimTypes.UserID);

        public static string GetRequiredClaim(this ClaimsPrincipal principal, string claimType)
        {
            var identity = (principal.Identity as ClaimsIdentity) ?? throw new Exception("Identity not found");

            var claim = identity.Claims.FirstOrDefault(c => c.Type == claimType) ?? throw new MissingRequiredClaimException(claimType);

            return claim.Value;
        }
    }
}
