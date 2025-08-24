using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Sen4.Exceptions;

namespace Sen4.Filters;

public class Idempotent(string bodyName, string requestIdName): Attribute, IAsyncActionFilter
{
    public IMemoryCache? MemoryCacheService { get; set; }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        MemoryCacheService = context.HttpContext.RequestServices.GetService<IMemoryCache>() ?? throw new IdempotencyFilterException("IMemoryCache service can not be null");
    
        var body = context.ActionArguments[bodyName];
        var headers = context.HttpContext.Request.Headers[requestIdName];
        if (headers.Count == 0)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);
            return;
        }

        var requestId = headers.First();
        if (body is not null && !string.IsNullOrEmpty(requestId))
        {
            var bodyString = JsonConvert.SerializeObject(body);
            using var sha256 = SHA256.Create();
            var bodyHashHexString = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(bodyString)));

            if (MemoryCacheService.TryGetValue(requestId, out string? cacheHashHexString) &&
                cacheHashHexString == bodyHashHexString)
                context.Result = new StatusCodeResult(StatusCodes.Status409Conflict);
            else
            {
                MemoryCacheService.Set(requestId, bodyHashHexString, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                await next();
            }
        }
    }
}