using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using IconnectDashboardGateway.Contracts.Dtos.Server;

namespace IconnectDashboardGateway.Application.Interfaces.Server
{
    public interface IServerService
    {
        Task<JsonResponseModel<List<ServerDto>>> GetSeverAsync(string siteId, CancellationToken cancellationToken = default);
    }
}
