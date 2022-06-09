using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMServer.Exceptions
{
    public class ServerError : ObjectResult
    {
        public ServerError(string message, int code) : base(Serialise(message, code))
        {
            StatusCode = code;
        }

        /// <summary>
        /// Replaces the response message to JSON {"Message": message, "StatusCode": code}
        /// </summary>
        private static string Serialise(string message, int code)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new { Message = message, StatusCode = code });
        }
    }

    public class InternalServerError : ServerError
    {
        public InternalServerError() : this("Internal Server Error")
        {

        }

        public InternalServerError(string value) : base(value, StatusCodes.Status500InternalServerError)
        {

        }
    }
}