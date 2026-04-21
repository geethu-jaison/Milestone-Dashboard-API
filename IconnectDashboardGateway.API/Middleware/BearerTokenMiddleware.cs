using IconnectDashboardGateway.Application.Interfaces.Auth;
using IconnectDashboardGateway.Application.Interfaces.Logger;

namespace IconnectDashboardGateway.API.Middleware
{
    public class BearerTokenMiddleware: IMiddleware
    {
        private readonly ISiteAuthService _siteAuthService;
        private readonly IAppLogger _appLogger;
        public BearerTokenMiddleware(ISiteAuthService siteAuthService,IAppLogger appLogger)
        {
            _siteAuthService = siteAuthService;
            _appLogger = appLogger;
        }

        /// <summary>
        /// Middleware entry point:
        /// - bypasses handshake/docs endpoints
        /// - enforces Authorization: Bearer <token>
        /// - validates token via SiteAuthService
        /// - returns 401 for missing/invalid/expired token
        /// </summary>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                var path = context.Request.Path.Value ?? string.Empty;
                // Bypass auth endpoint + swagger/scalar
                if (path.StartsWith("/api/v1/auth/handshake", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase))
                {
                    await next(context);
                    return;
                }
                var authHeader = context.Request.Headers.Authorization.ToString();
                if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { status = "Error", message = "Missing bearer token." });
                    return;
                }
                var token = authHeader["Bearer ".Length..].Trim();
                if (string.IsNullOrWhiteSpace(token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { status = "Error", message = "Invalid bearer token." });
                    return;
                }
                var isValid = await _siteAuthService.ValidateTokenAsync(token, context.RequestAborted);
                if (!isValid)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { status = "Error", message = "Token invalid or expired." });
                    return;
                }
                await next(context);
            
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                _appLogger.LogError("Authentication failed in BearerTokenMiddleware.", ex);
                return;
            }
        }
    }
}
