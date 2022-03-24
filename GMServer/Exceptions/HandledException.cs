using System;

namespace GMServer.Exceptions
{
    public class HandledException : Exception
    {
        public int StatusCode { get; set; }    

        public HandledException(string message, int status) : base(message)
        {
            StatusCode = status;
        }
    }
}
