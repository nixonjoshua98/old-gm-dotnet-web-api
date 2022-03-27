using GMServer.Authentication;
using GMServer.Exceptions;
using System.Linq;
using System.Security.Claims;

namespace GMServer.Extensions
{
    public static class ClaimsIdentityExtensions
    {
        public static string UserID(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(claim => claim.Type == ClaimNames.UserID)?.Value ?? throw new MissingRequiredClaimException(ClaimNames.UserID);
        }
    }
}
