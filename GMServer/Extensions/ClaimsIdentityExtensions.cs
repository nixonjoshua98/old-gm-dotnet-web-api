using System.Security.Claims;
using GMServer.Authentication;
using System.Linq;
using GMServer.Exceptions;

namespace GMServer.Extensions
{
    public static class ClaimsIdentityExtensions
    {
        public static string UserID (this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(claim => claim.Type == ClaimNames.UserID)?.Value ?? throw new MissingRequiredClaimException(ClaimNames.UserID);
        }
    }
}
