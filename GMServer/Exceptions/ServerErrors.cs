using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMServer.Exceptions
{
    public class ServerError : ObjectResult
    {
        public ServerError(object value, int statusCode) : base(value)
        {
            StatusCode = statusCode;
        }
    }

    public class InternalServerError : ServerError
    {
        public InternalServerError(object value) : base(value, StatusCodes.Status500InternalServerError)
        {

        }
    }
}
