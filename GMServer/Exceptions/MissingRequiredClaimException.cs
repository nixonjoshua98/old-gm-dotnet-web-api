using System;

namespace GMServer.Exceptions
{
    public class MissingRequiredClaimException : Exception
    {
        public MissingRequiredClaimException(string claim) : base($"Required claim '{claim}' was not found")
        {

        }
    }
}
