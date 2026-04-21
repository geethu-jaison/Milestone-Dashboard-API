using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using IconnectDashboardGateway.Application.Interfaces.Connection;
using IconnectDashboardGateway.Application.Interfaces.DataAccess;
using IconnectDashboardGateway.Application.Interfaces.Logger;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using IconnectDashboardGateway.Contracts.Dtos.Server;
using Microsoft.Data.SqlClient;

namespace IconnectDashboardGateway.Infrastructure.DataAccess
{
    [Obfuscation(ApplyToMembers = true, Exclude = false)]
    public class ServerRepository : IServerRepository
    {
        private readonly IAppLogger _appLogger;
        private readonly IRegistryConnectionStringProvider _registryConnectionStringProvider;
        public ServerRepository(IAppLogger appLogger,IRegistryConnectionStringProvider registryConnectionStringProvider)
        {
            _appLogger = appLogger;
            _registryConnectionStringProvider = registryConnectionStringProvider;
        }

        public async Task<JsonResponseModel<List<ServerDto>>> GetServerAsync(string siteId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(siteId))
                    return JsonResponseModel<List<ServerDto>>.Fail("siteId is required.");
                await using var con = new SqlConnection(_registryConnectionStringProvider.GetConnectionString());
                await con.OpenAsync(cancellationToken);

                const string existsSql = @"
                                            IF EXISTS (SELECT 1 FROM dbo.CameraCache WHERE SiteId = @SiteId)
                                                       SELECT 1;
                                                              ELSE
                                                       SELECT 0;
                                            ";
                var siteExists=await con.QuerySingleAsync<int>(new CommandDefinition(existsSql, new { SiteId = siteId }, cancellationToken: cancellationToken));
                if (siteExists == 0)
                    return JsonResponseModel<List<ServerDto>>.Fail("Site not found.");
                 const string sqlList = @"
                                            SELECT
                                                ServerName,
                                                Status,
                                                Health,
                                                CAST(UsedGB AS nvarchar(50)) AS UsedGB,
                                                CAST(TotalGB AS nvarchar(50)) AS TotalGB,
                                                RetentionTime
                                            FROM dbo.Servers
                                            WHERE SiteId = @SiteId;";
                var servers=(await con.QueryAsync<ServerDto>(new CommandDefinition(sqlList, new { SiteId = siteId }, cancellationToken: cancellationToken))).AsList();                            

                return JsonResponseModel<List<ServerDto>>.Ok(servers, "server List Retrieved Successfully");
            }
            catch (Exception ex)
            {
                _appLogger.LogError($"Error fetching server for siteId={siteId}. Error: {ex.Message}", ex);
                return JsonResponseModel<List<ServerDto>>.Fail("Unable to load server information.");
            }
        }
    }
}
