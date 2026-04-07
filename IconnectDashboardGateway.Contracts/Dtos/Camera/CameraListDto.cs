using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IconnectDashboardGateway.Contracts.Dtos.Camera
{
    public class CameraListDto
    {
        //public UniqueId? CameraId { get; set; }
        public string? CameraName { get; set; }
        public string? Group { get; set; }
        public int? PasswordStrength { get; set; }
        public int? StorageStatus { get; set; }
        public string? Status { get; set; }
        public string? CameraHealth { get; set; }
        public string? MacAddress { get; set; }
        public string? IPAddress { get; set; }
        public string? DiskPath { get; set; }
        public string? StorageUsed { get; set; }
        public string? RemainingStorage { get; set; }
        public string? AllotedStorage { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
    }
}
