using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconnectDashboardGateway.Contracts.Dtos.Camera
{
    public class CameraDto
    {
        public int TotalCameras { get; set; }
        public int OnlineCameras { get; set; }
        public int OfflineCameras { get; set; }
    }
}
