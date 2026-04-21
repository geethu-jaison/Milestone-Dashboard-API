using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Contracts.Dtos.Auth;
using IconnectDashboardGateway.Contracts.Dtos.Common;

namespace IconnectDashboardGateway.Application.Interfaces.Auth
{
    public interface ISiteAuthService
    {
        Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default);
        Task<JsonResponseModel<ParentHandshakeResponseDto>> ValidateHandshakeAndIssueTokenAsync(string payload, CancellationToken cancellationToken = default);
    }
}
