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

    public class ExpiredTokenException : ServerException
    {
        public ExpiredTokenException() : base("Unauthorized", (int)HttpStatusCode.Unauthorized)
        {

        }
    }
}
