using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Application.Interfaces.DataAccess;
using IconnectDashboardGateway.Application.Interfaces.Logger;
using IconnectDashboardGateway.Application.Interfaces.Server;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using IconnectDashboardGateway.Contracts.Dtos.Server;

namespace IconnectDashboardGateway.Application.Services
{
    public class ServerService : IServerService
    {
        private readonly IServerRepository _serverRepository;
        private readonly IAppLogger _appLogger;
        public ServerService(IServerRepository serverRepository, IAppLogger appLogger)
        {
            _serverRepository = serverRepository;
            _appLogger = appLogger;
        }

        public async Task<JsonResponseModel<List<ServerDto>>> GetSeverAsync(string siteId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _serverRepository.GetServerAsync(siteId, cancellationToken);
                if (response.Status == "Success" && response.Data is not null)
                    return JsonResponseModel<List<ServerDto>>.Ok(response.Data, "Successfully got server information");
                return response; // already Fail or empty from repository
            }
            catch (Exception ex)
            {
                _appLogger.LogError($"Server info retrieval failed for siteId={siteId}, error={ex.Message}", ex);
                return JsonResponseModel<List<ServerDto>>.Fail("Failed to get the server list");

            }
        }
    }
}
