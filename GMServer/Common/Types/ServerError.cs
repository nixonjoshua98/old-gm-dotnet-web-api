using Microsoft.AspNetCore.Mvc;

namespace GMServer.Common.Types
{
    public class ServerError : ObjectResult
    {
        public static readonly ServerError InternalServerError = new("Internal server error", 500);

        public ServerError(string message, int code) : base(Serialise(message, code))
        {
            StatusCode = code;
        }

        private static string Serialise(string message, int code)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new { Message = message, StatusCode = code });
        }
    }
}