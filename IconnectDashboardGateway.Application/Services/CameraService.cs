using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Application.Interfaces.Camera;
using IconnectDashboardGateway.Application.Interfaces.Logger;
using IconnectDashboardGateway.Application.Interfaces.Repositories;
using IconnectDashboardGateway.Contracts.Dtos.Camera;
using IconnectDashboardGateway.Contracts.Dtos.Common;
using IconnectDashboardGateway.Contracts.Enums;

namespace IconnectDashboardGateway.Application.Services
{
    public class CameraService:ICameraService
    {
        public ICameraRepository _cameraRepository;
        public IAppLogger _appLogger;
        public CameraService(ICameraRepository cameraRepository,IAppLogger appLogger)
        {
            _appLogger = appLogger;
            _cameraRepository = cameraRepository;
        }

        #region camera summary
        public async Task<JsonResponseModel<CameraDto>> GetCameraSummary(string siteId, CancellationToken cancellation = default)
        {
            try
            {
                var response = await _cameraRepository.GetCameraSummary(siteId, cancellation);

                if (response.Status == "Success" && response.Data is not null)
                    return JsonResponseModel<CameraDto>.Ok(response.Data, "Successfully got camera summary");

                return response; // already Fail or empty from repository
            }
            catch (Exception ex)
            {
                _appLogger.LogError($"Camera summary failed for siteId={siteId}, error={ex.Message}", ex);
                return JsonResponseModel<CameraDto>.Fail($"Unable to load camera summary.");
            }
        }
        #endregion

        #region camera list with filter
        public async Task<JsonResponseModel<CameraListResponseDto>> GetCamerasAsync(string siteId, CameraListFilter filter, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cameraRepository.GetCamerasAsync(siteId, filter, cancellationToken);
            }
            catch (Exception ex)
            {
                _appLogger.LogError($"Camera list failed for siteId={siteId}, error={ex.Message}", ex);
                return JsonResponseModel<CameraListResponseDto>.Fail("Unable to load camera list.");
            }
        }
        #endregion
    }
}
