using System;
using System.Linq;

namespace GMServer.Authentication
{
    public static class AuthUtility
    {
        public static string GenerateAccessToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, 64)
                             .Select(s => s[Random.Shared.Next(s.Length)])
                             .ToArray());
        }
    }
}
