using System;

namespace SRC.Common.Exceptions
{
    public class MissingRequiredClaimException : Exception
    {
        public MissingRequiredClaimException(string claim) : base($"Required claim '{claim}' was not found")
        {

        }
    }
}
