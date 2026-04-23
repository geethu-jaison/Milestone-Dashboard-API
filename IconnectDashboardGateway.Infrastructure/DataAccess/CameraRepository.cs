using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using IconnectDashboardGateway.Application.Interfaces.Connection;
using IconnectDashboardGateway.Application.Interfaces.Logger;
using IconnectDashboardGateway.Application.Interfaces.Repositories;
using IconnectDashboardGateway.Contracts.Dtos.Camera;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using IconnectDashboardGateway.Contracts.Enums;
using Microsoft.Data.SqlClient;

namespace IconnectDashboardGateway.Infrastructure.DataAccess
{
    [Obfuscation(Exclude = false, ApplyToMembers = true)]
    public class CameraRepository:ICameraRepository
    {
        private readonly IRegistryConnectionStringProvider _connectionStringProvider;
        private readonly IAppLogger _appLogger;
        public CameraRepository(IRegistryConnectionStringProvider registryConnectionStringProvider,IAppLogger appLogger)
        {
            _connectionStringProvider = registryConnectionStringProvider;
            _appLogger = appLogger;
        }

        #region Camera Summary
        public async Task<JsonResponseModel<CameraDto>> GetCameraSummary(string siteId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteId))
                    return JsonResponseModel<CameraDto>.Fail("siteId is required.");
                const string sql = @"
                                DECLARE @Ok BIT = CASE WHEN EXISTS (SELECT 1 FROM dbo.SiteInfo WHERE SiteId = @SiteId) THEN 1 ELSE 0 END;
                                IF @Ok = 0
                                      SELECT
                                             CAST(0 AS INT) AS TotalCameras,
                                             CAST(0 AS INT) AS OfflineCameras,
                                             CAST(0 AS INT) AS OnlineCameras,
                                             CAST(0 AS INT) AS WeakPasswordCameras,
                                             CAST(@Ok AS INT) AS SiteExists;
                                       ELSE
                                       SELECT
                                             COUNT(1) AS TotalCameras,
                                             SUM(CASE WHEN Status = 'Offline' THEN 1 ELSE 0 END) AS OfflineCameras,
                                             SUM(CASE WHEN Status = 'Online' THEN 1 ELSE 0 END) AS OnlineCameras,
                                             SUM(CASE WHEN PasswordStrength = 1 THEN 1 ELSE 0 END) AS WeakPasswordCameras,
                                             CAST(@Ok AS INT) AS SiteExists
                                       FROM dbo.CameraCache
                                WHERE SiteId = @SiteId;";

                await using var connection = new SqlConnection(_connectionStringProvider.GetConnectionString());
                await connection.OpenAsync(cancellationToken);
                var (total, offline, online, weakPasswordCameras, siteExistsFlag) =
                    await connection.QuerySingleAsync<(int TotalCameras, int OfflineCameras, int OnlineCameras, int weakPasswordCameras, int SiteExists)>(
                        new CommandDefinition(sql, new { SiteId = siteId }, cancellationToken: cancellationToken));
                if (siteExistsFlag == 0)
                    return JsonResponseModel<CameraDto>.Fail("Site not found.");
                var dto = new CameraDto
                {
                    TotalCameras = total,
                    OfflineCameras = offline,
                    OnlineCameras = online,
                    WeakPasswordCameras = weakPasswordCameras
                };
                return JsonResponseModel<CameraDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _appLogger.LogError($"Error fetching camera summary for siteId={siteId}. Error: {ex.Message}", ex);
                return JsonResponseModel<CameraDto>.Fail($"Unable to load camera summary.");
            }
        }
        #endregion

        #region Camera list with filters
        public async Task<JsonResponseModel<CameraListResponseDto>> GetCamerasAsync(string siteId, CameraListFilter filter, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteId))
                    return JsonResponseModel<CameraListResponseDto>.Fail("siteId is required.");

                if (!Enum.IsDefined(typeof(CameraListFilter), filter))
                    return JsonResponseModel<CameraListResponseDto>.Fail("Invalid filter.");

                await using var connection = new SqlConnection(_connectionStringProvider.GetConnectionString());
                await connection.OpenAsync(cancellationToken);

                const string existsSql = @"
                                            IF EXISTS (SELECT 1 FROM dbo.SiteInfo WHERE SiteId = @SiteId)
                                                SELECT 1;
                                            ELSE
                                                SELECT 0;";

                var exists = await connection.QuerySingleAsync<int>(
                    new CommandDefinition(existsSql, new { SiteId = siteId }, cancellationToken: cancellationToken));

                if (exists == 0)
                    return JsonResponseModel<CameraListResponseDto>.Fail("Site not found.");

                const string listSql = @"
                                            SELECT c.*
                                            FROM dbo.CameraCache c
                                            WHERE c.SiteId = @SiteId
                                              AND (
                                                    @Filter = 0
                                                 OR (@Filter = 1 AND c.Status = N'Offline')
                                                 OR (@Filter = 2 AND c.Status = N'Online')
                                                 OR (@Filter = 3 AND c.PasswordStrength = 1)
                                                  )
                                            ORDER BY c.CameraName;";

                var rows = (await connection.QueryAsync<CameraListDto>(
                    new CommandDefinition(
                        listSql,
                        new { SiteId = siteId, Filter = (int)filter },
                        cancellationToken: cancellationToken)))
                    .AsList();

                var payload = new CameraListResponseDto
                {
                    SiteId = siteId,
                    Filter = filter,
                    Cameras = rows
                };

                var filterLabel = filter switch
                {
                    CameraListFilter.All => "all cameras",
                    CameraListFilter.Offline => "offline cameras",
                    CameraListFilter.Online => "online cameras",
                    CameraListFilter.WeakPassword => "weak-password cameras",
                    _ => "cameras"
                };

                var message = $"Successfully fetched {rows.Count} {filterLabel}.";
                return JsonResponseModel<CameraListResponseDto>.Ok(payload, message);
            }
            catch (Exception ex)
            {
                _appLogger.LogError($"Error fetching camera list for siteId={siteId}. Error: {ex.Message}", ex);
                return JsonResponseModel<CameraListResponseDto>.Fail("Unable to load camera list.");
            }
        }
        #endregion
    }
}
