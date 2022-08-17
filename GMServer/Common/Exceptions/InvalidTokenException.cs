using System;

namespace SRC.Common.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException() : base("Unauthorized") { }
    }
}
