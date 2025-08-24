namespace Sen4.Middlewares;

public class LogRequestMiddleware(ILogger<LogRequestMiddleware> logger): IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        IFormCollection? form = null;
        if (context.Request.HasFormContentType) 
            form = await context.Request.ReadFormAsync();
        
        var logInfo = new
        {
            Headers = context.Request.Headers.ToDictionary(h => h.Key, h => string.Join("; ", h.Value)),
            QueryParams = context.Request.Query.ToDictionary(q => q.Key, q => string.Join(", ", q.Value)),
            Method = context.Request.Method,
            Path = context.Request.Path.ToString(),
            Form = form,
        };
            
        var logJson = System.Text.Json.JsonSerializer.Serialize(logInfo, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        logger.LogInformation("Request details: {logJson}", logJson);
            
        await next.Invoke(context);
    }
}