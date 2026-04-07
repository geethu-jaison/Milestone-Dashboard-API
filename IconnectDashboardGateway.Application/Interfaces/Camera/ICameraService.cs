using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Contracts.Dtos.Camera;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using IconnectDashboardGateway.Contracts.Enums;

namespace IconnectDashboardGateway.Application.Interfaces.Camera
{
    public interface ICameraService
    {
        Task<JsonResponseModel<CameraDto>> GetCameraSummary(string siteId, CancellationToken cancellationToken = default);
        Task<JsonResponseModel<CameraListResponseDto>> GetCamerasAsync(string siteId,CameraListFilter filter,CancellationToken cancellationToken = default);
    }
}
