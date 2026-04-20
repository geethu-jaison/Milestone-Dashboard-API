using IconnectDashboardGateway.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace IconnectDashboardGateway.API.Controllers
{
    [ApiController]
    [Route("/api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ISiteAuthService _siteAuthService;
        private readonly ITokenService _tokenService;
        public AuthController(ISiteAuthService siteAuthService,ITokenService tokenService)
        {
            _siteAuthService = siteAuthService;
            _tokenService = tokenService;
        }
    }
}
