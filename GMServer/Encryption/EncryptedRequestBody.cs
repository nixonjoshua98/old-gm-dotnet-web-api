using GMServer.Common;
using GMServer.Models.Settings;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GMServer.Encryption
{
    public class EncryptedRequestBody : Attribute, IAsyncResourceFilter
    {
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            EncryptionSettings settings = context.HttpContext.RequestServices.GetService<EncryptionSettings>();

            var request = context.HttpContext.Request;

            string cipherRequest;

            using (var reader = new StreamReader(request.Body))
            {
                cipherRequest = await reader.ReadToEndAsync();
            }

            string plainRequest = AES.Decrypt(cipherRequest, settings);

            request.Body = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(plainRequest));

            await next();
        }
    }
}
