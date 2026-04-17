using IconnectDashboardGateway.Application.Interfaces.Site;
using Microsoft.AspNetCore.Mvc;

namespace IconnectDashboardGateway.API.Controllers
{
    [ApiController]
    [Route("api/v1/sites/{siteId}/Site")]
    public class SiteController : ControllerBase
    {
        private readonly ISiteService _siteService;
        public SiteController(ISiteService siteService)
        {
            _siteService = siteService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetSite(string siteId, CancellationToken cancellationToken)
        {
            var response = await _siteService.GetSiteAsync(siteId, cancellationToken);
            if (response.Status == "Error")
                return BadRequest(response);
            return Ok(response);
        }
    }
}
