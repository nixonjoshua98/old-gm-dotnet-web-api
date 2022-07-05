using GMServer.Models.Settings;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GMServer.Encryption
{
    public class EncryptedRequestBody : Attribute, IAsyncResourceFilter
    {
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            EncryptionSettings settings = context.HttpContext.RequestServices.GetService<EncryptionSettings>();

            HttpRequest request = context.HttpContext.Request;

            request.Body = await DecryptRequest(request, settings);

            await next();
        }

        async Task<MemoryStream> DecryptRequest(HttpRequest request, EncryptionSettings settings)
        {
            string cipherRequest;

            using (var reader = new StreamReader(request.Body))
            {
                cipherRequest = await reader.ReadToEndAsync();
            }

            string plainRequest = AES.Decrypt(cipherRequest, settings);

            return new MemoryStream(System.Text.Encoding.ASCII.GetBytes(plainRequest));
        }
    }
}
