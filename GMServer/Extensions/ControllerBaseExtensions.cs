using GMServer.Exceptions;
using GMServer.MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GMServer.Extensions
{
    public static class ControllerBaseExtensions
    {
        public static IActionResult ResponseOrError<T>(this ControllerBase controller, T response) where T : BaseResponseWithError
        {
            if (response.Success)
                return new OkObjectResult(response);

            return new ServerError(response.Message, response.StatusCode);
        }
    }
}
