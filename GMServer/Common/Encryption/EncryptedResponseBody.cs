using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SRC.Models.Settings;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SRC.Common.Encryption
{
    public class EncryptedResponseBody : Attribute, IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            EncryptionSettings settings = context.HttpContext.RequestServices.GetRequiredService<EncryptionSettings>();

            var response = context.HttpContext.Response;

            var existingBody = response.Body;

            using (var ms = new MemoryStream())
            {
                response.Body = ms;

                await next();

                ms.Seek(0, SeekOrigin.Begin);

                string responseBody = System.Text.Encoding.UTF8.GetString(ms.ToArray());

                response.Body = existingBody;

                string newContent = AES.Encrypt(responseBody, settings);

                response.Headers["Response-Encrypted"] = "true";

                // (Required) Request will not send if the content length is not updated
                response.Headers["Content-Length"] = newContent.Length.ToString();

                await response.WriteAsync(newContent);
            }
        }
    }
}