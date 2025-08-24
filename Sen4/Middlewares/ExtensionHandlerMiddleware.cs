namespace Sen4.Middlewares;

public class ExtensionHandlerMiddleware(ILogger<ExtensionHandlerMiddleware> logger): IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            const string message = "An unhandled exception has occurred while executing the request.";
            
            logger.LogError(exception.Message);
            
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}