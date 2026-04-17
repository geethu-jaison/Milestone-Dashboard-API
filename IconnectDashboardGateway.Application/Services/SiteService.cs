using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Application.Interfaces.DataAccess;
using IconnectDashboardGateway.Application.Interfaces.Logger;
using IconnectDashboardGateway.Application.Interfaces.Repositories;
using IconnectDashboardGateway.Application.Interfaces.Site;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using IconnectDashboardGateway.Contracts.Dtos.Site;

namespace IconnectDashboardGateway.Application.Services
{
    public class SiteService:ISiteService
    {
        private readonly IAppLogger _appLogger;
        private readonly ISiteRepository _siteRepository;
        public SiteService(IAppLogger appLogger,ISiteRepository siteRepository) 
        {
            _appLogger=appLogger;
            _siteRepository=siteRepository;
        }
        public async Task<JsonResponseModel<SiteDto?>> GetSiteAsync(string siteId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _siteRepository.GetSiteInfoAsync(siteId, cancellationToken);
            }
            catch (Exception ex)
            {
                _appLogger.LogError($"Site retrieval failed for siteId={siteId}, error={ex.Message}", ex);
                return JsonResponseModel<SiteDto?>.Fail("Failed to load site information.");
            }
        }
    }
}
