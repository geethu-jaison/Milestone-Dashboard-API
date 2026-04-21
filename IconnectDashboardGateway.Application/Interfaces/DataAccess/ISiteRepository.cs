using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using IconnectDashboardGateway.Contracts.Dtos.Site;

namespace IconnectDashboardGateway.Application.Interfaces.DataAccess
{
    public interface ISiteRepository
    {
        Task<JsonResponseModel<SiteDto>> GetSiteInfoAsync(string siteId, CancellationToken cancellationToken = default);
    }
}
