using Microsoft.Extensions.Primitives;
using System.Net;

namespace PterodactylPavlovServerController.Middleware
{
    public class BasicApiKeyMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IConfiguration configuration;

        public BasicApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.next = next;
            this.configuration = configuration;
        }

        public Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("x-api-key", out StringValues apiKeyValues) || apiKeyValues.Count != 1 || apiKeyValues.First() != configuration["apikey"])
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Task.CompletedTask;
            }

            return this.next.Invoke(context);
        }
    }
}
