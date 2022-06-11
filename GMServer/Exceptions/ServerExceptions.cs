using System;
using System.Net;

namespace GMServer.Exceptions
{
    public class ServerException : Exception
    {
        public int StatusCode;

        public ServerException(string message, int status) : base(message)
        {
            StatusCode = status;
        }
    }

    public class InvalidTokenException : ServerException
    {
        public InvalidTokenException() : base("Unauthorized", (int)HttpStatusCode.Unauthorized) { }
    }
}
