using IconnectDashboardGateway.Application.Interfaces.Camera;
using IconnectDashboardGateway.Contracts.Dtos.Camera;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using Microsoft.AspNetCore.Mvc;

namespace IconnectDashboardGateway.API.Controllers
{
    [ApiController]
    [Route("api/v1/sites/{siteId}/cameras")]
    public class CameraController : ControllerBase
    {
        private readonly ICameraService _cameraService;
        public CameraController(ICameraService cameraService)
        {
            _cameraService = cameraService;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<JsonResponseModel<CameraDto>>> GetCameraSummary(string siteId,CancellationToken cancellationToken=default)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                return BadRequest("site id is required");

            var response = await _cameraService.GetCameraSummary(siteId);

            if (response.Status == "Success")
                return Ok(response);
            else
                return Ok(response);
        }
    }
}
  