using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using IconnectDashboardGateway.Contracts.Dtos.Site;

namespace IconnectDashboardGateway.Application.Interfaces.Site
{
    public interface ISiteService
    {
        Task<JsonResponseModel<SiteDto?>> GetSiteAsync(string siteId, CancellationToken cancellationToken = default);
    }
}
