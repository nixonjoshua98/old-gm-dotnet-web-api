using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GMServer.Encryption
{
    public class EncryptedRequestBody : Attribute, IAsyncResourceFilter
    {
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var request = context.HttpContext.Request;

            //request.Body = new MemoryStream(Encoding.ASCII.GetBytes("{\"id\":10}"));

            await next();
        }
    }
}
