using Microsoft.Extensions.Primitives;
using System.Net;

namespace PterodactylPavlovServerController.Middleware;

public class BasicApiKeyMiddleware
{
    private readonly IConfiguration configuration;
    private readonly RequestDelegate next;

    public BasicApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        this.next = next;
        this.configuration = configuration;
    }

    public Task Invoke(HttpContext context)
    {
        PathString path = context.Request.Path;
        if (path != "/api/Stats")
        {
            if (!context.Request.Headers.TryGetValue("x-api-key", out StringValues apiKeyValues) || apiKeyValues.Count != 1 || apiKeyValues.First() != this.configuration["apikey"])
            {
                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                return Task.CompletedTask;
            }

            if (!context.Request.Headers.TryGetValue("x-pterodactyl-api-key", out StringValues pterodactylApiKeyValues) || pterodactylApiKeyValues.Count != 1)
            {
                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                return Task.CompletedTask;
            }
        }

        return this.next.Invoke(context);
    }
}
