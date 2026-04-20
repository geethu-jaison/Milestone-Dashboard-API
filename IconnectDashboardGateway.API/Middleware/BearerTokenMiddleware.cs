namespace IconnectDashboardGateway.API.Middleware
{
    public class BearerTokenMiddleware:IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await next(context);
        }
    }
}
