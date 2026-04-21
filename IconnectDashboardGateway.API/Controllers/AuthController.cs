using IconnectDashboardGateway.Application.Interfaces.Auth;
using IconnectDashboardGateway.Contracts.Dtos.Auth;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IconnectDashboardGateway.API.Controllers
{
    [ApiController]
    [Route("/api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ISiteAuthService _siteAuthService;
        public AuthController(ISiteAuthService siteAuthService)
        {
            _siteAuthService = siteAuthService;
        }
        [HttpPost("handshake")]
        [AllowAnonymous]
        public async Task<IActionResult> Handshake([FromBody] ParentHandshakeResultDto resultDto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(resultDto.payload))
                return BadRequest(JsonResponseModel<ParentHandshakeResultDto>.Fail("Payload is required."));

            var response = await _siteAuthService.ValidateHandshakeAndIssueTokenAsync(resultDto.payload, cancellationToken);

            if (response.Status == "Error")
                return Unauthorized(response);

            return Ok(response);
        }
    }
}
