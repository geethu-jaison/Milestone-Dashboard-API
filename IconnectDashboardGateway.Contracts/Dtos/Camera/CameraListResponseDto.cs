using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Contracts.Enums;

namespace IconnectDashboardGateway.Contracts.Dtos.Camera
{
    public class CameraListResponseDto
    {
        public string SiteId { get; set; } = default!;
        public CameraListFilter Filter { get; set; }
        public IReadOnlyList<CameraListDto> Cameras { get; set; } = Array.Empty<CameraListDto>();
    }
}

