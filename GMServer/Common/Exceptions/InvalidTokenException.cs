using System;

namespace GMServer.Common.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException() : base("Unauthorized") { }
    }
}
