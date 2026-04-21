using System.Runtime.InteropServices;
using IconnectDashboardGateway.Application.Interfaces.Server;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using IconnectDashboardGateway.Contracts.Dtos.Server;
using Microsoft.AspNetCore.Mvc;

namespace IconnectDashboardGateway.API.Controllers
{
    [ApiController]
    [Route("api/v1/sites/{siteId}/servers")]
    public class ServerController : ControllerBase
    {
        private readonly IServerService _serverService;
        public ServerController(IServerService serverService)
        {
            _serverService = serverService;
        }

        [HttpGet]
        public async Task<ActionResult<JsonResponseModel<List<ServerDto>>>> GetServersAsync(string siteId,CancellationToken cancellationToken=default)
        {
            if(string.IsNullOrEmpty(siteId))
                return BadRequest("site id is required");
            var response = await _serverService.GetSeverAsync(siteId, cancellationToken);
            if (response.Status == "Success")
                return Ok(response);
            return Ok(response);
        }
    }
}
