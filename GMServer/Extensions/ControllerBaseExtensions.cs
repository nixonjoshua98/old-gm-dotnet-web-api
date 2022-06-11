using GMServer.MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GMServer.Extensions
{
    public static class ControllerBaseExtensions
    {
        public static IActionResult ResponseOrError<T>(this ControllerBase controller, T resp) where T : AbstractResponseWithError
        {
            if (resp.Success)
                return controller.Ok(resp);

            return resp.Error;
        }
    }
}
