using GMServer.Exceptions;
using Newtonsoft.Json;

namespace GMServer.MediatR
{
    public abstract class AbstractResponseWithError
    {
        [JsonIgnore]
        public bool Success { get => Error is null; }

        [JsonIgnore]
        public ServerError Error;

        public AbstractResponseWithError()
        {

        }

        public AbstractResponseWithError(string message, int code)
        {
            Error = new(message, code);
        }
    }
}
