using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using IconnectDashboardGateway.Application.Interfaces.Connection;
using IconnectDashboardGateway.Application.Interfaces.DataAccess;
using IconnectDashboardGateway.Application.Interfaces.Logger;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using IconnectDashboardGateway.Contracts.Dtos.Site;
using Microsoft.Data.SqlClient;

namespace IconnectDashboardGateway.Infrastructure.DataAccess
{

    [Obfuscation(ApplyToMembers = true, Exclude = false)]
    public class SiteRepository : ISiteRepository
    {
        private readonly IAppLogger _appLogger;
        private readonly IRegistryConnectionStringProvider _ConnectionStringProvider;
        public SiteRepository(IAppLogger appLogger, IRegistryConnectionStringProvider connectionStringProvider)
        {
            _appLogger = appLogger;
            _ConnectionStringProvider = connectionStringProvider;
        }

        public async Task<JsonResponseModel<SiteDto>> GetSiteInfoAsync(string siteId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteId))
                    return JsonResponseModel<SiteDto>.Fail("site id is required");
                const string sql = @"
                                  SELECT
                                        SiteId,
                                        SiteType,
                                        SiteName,
                                        SiteApiAddress,
                                        SiteDescription,
                                        SiteData,
                                        DateAddedUtc,
                                        DateModifiedUtc
                                    FROM dbo.SiteInfo
                                    WHERE SiteId = @SiteId;";
                await using var con = new SqlConnection(_ConnectionStringProvider.GetConnectionString());
                await con.OpenAsync(cancellationToken);
                var row = await con.QuerySingleOrDefaultAsync<SiteDto>(
                  new CommandDefinition(sql, new { SiteId = siteId }, cancellationToken: cancellationToken));
                if (row is null)
                    return JsonResponseModel<SiteDto?>.Fail("Site not found.");
                return JsonResponseModel<SiteDto?>.Ok(row, "Site retrieved successfully.");
            }
            catch (Exception ex)
            {
                _appLogger.LogError($"Error in getting site info  for SiteId: {siteId}", ex);
                return JsonResponseModel<SiteDto>.Fail("An error occurred while retrieving site information.");
            }
        }
    }
}
