using Newtonsoft.Json;

namespace GMServer.MediatR
{
    public abstract class BaseResponseWithError
    {
        [JsonIgnore]
        public bool Success = true;

        [JsonIgnore]
        public int StatusCode = 200;

        [JsonIgnore]
        public string Message = string.Empty;

        public BaseResponseWithError()
        {

        }

        public BaseResponseWithError(string message, int code)
        {
            Success = false;
            StatusCode = code;
            Message = message;
        }
    }
}
