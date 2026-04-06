using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using IconnectDashboardGateway.Application.Interfaces.Connection;
using IconnectDashboardGateway.Application.Interfaces.Logger;
using IconnectDashboardGateway.Application.Interfaces.Repositories;
using IconnectDashboardGateway.Contracts.Dtos.Camera;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using Microsoft.Data.SqlClient;

namespace IconnectDashboardGateway.Infrastructure.DataAccess
{
    public class CameraRepository:ICameraRepository
    {
        private readonly IRegistryConnectionStringProvider _connectionStringProvider;
        private readonly IAppLogger _appLogger;
        public CameraRepository(IRegistryConnectionStringProvider registryConnectionStringProvider,IAppLogger appLogger)
        {
            _connectionStringProvider = registryConnectionStringProvider;
            _appLogger = appLogger;
        }

        public async Task<JsonResponseModel<CameraDto>> GetCameraSummary(string siteId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(siteId))
                return JsonResponseModel<CameraDto>.Fail("siteId is required.");
            const string sql = @"
                                DECLARE @Ok BIT = CASE WHEN EXISTS (SELECT 1 FROM dbo.CameraCache WHERE SiteId = @SiteId) THEN 1 ELSE 0 END;
                                IF @Ok = 0
                                      SELECT
                                             CAST(0 AS INT) AS TotalCameras,
                                             CAST(0 AS INT) AS OfflineCameras,
                                             CAST(0 AS INT) AS OnlineCameras,
                                             CAST(@Ok AS INT) AS SiteExists;
                                       ELSE
                                       SELECT
                                             COUNT(1) AS TotalCameras,
                                             SUM(CASE WHEN Status = 'Offline' THEN 1 ELSE 0 END) AS OfflineCameras,
                                             SUM(CASE WHEN Status = 'Online' THEN 1 ELSE 0 END) AS OnlineCameras,
                                             CAST(@Ok AS INT) AS SiteExists
                                       FROM dbo.CameraCache
                                WHERE SiteId = @SiteId;";

            await using var connection = new SqlConnection(_connectionStringProvider.GetConnectionString());
            await connection.OpenAsync(cancellationToken);
            var (total, offline, online, siteExistsFlag) =
                await connection.QuerySingleAsync<(int TotalCameras, int OfflineCameras, int OnlineCameras, int SiteExists)>(
                    new CommandDefinition(sql, new { SiteId = siteId }, cancellationToken: cancellationToken));
            if (siteExistsFlag == 0)
                return JsonResponseModel<CameraDto>.Fail("Site not found.");
            var dto = new CameraDto
            {
                TotalCameras = total,
                OfflineCameras = offline,
                OnlineCameras = online
            };
            return JsonResponseModel<CameraDto>.Ok(dto);
        }
    }
}
