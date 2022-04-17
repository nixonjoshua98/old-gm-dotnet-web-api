using Newtonsoft.Json;

namespace GMServer.MediatR
{
    public abstract class AbstractResponseWithError
    {
        [JsonIgnore]
        public bool Success = true;

        [JsonIgnore]
        public int StatusCode = 200;

        [JsonIgnore]
        public string Message = string.Empty;

        public AbstractResponseWithError()
        {

        }

        public AbstractResponseWithError(string message, int code)
        {
            Success = false;
            StatusCode = code;
            Message = message;
        }
    }
}
