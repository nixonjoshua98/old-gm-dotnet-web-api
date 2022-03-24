using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GMServer.Encryption
{
    public class EncryptedResponseBody : Attribute, IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var existingBody = context.HttpContext.Response.Body;

            using (var ms = new MemoryStream())
            {
                //context.HttpContext.Response.Body = ms;

                await next();

                ms.Seek(0, SeekOrigin.Begin);

                context.HttpContext.Response.Body = existingBody;

                //string newContent = "X";

                //await context.HttpContext.Response.WriteAsync(newContent);
            }
        }
    }
}
