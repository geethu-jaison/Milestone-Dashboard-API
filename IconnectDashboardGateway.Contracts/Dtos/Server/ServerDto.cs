using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconnectDashboardGateway.Contracts.Dtos.Server
{
    public class ServerDto
    {
        public string? ServerName { get; set; }
        public string? Status { get; set; }
        public string? Health { get; set; }
        public string? UsedGB { get; set; }
        public string? TotalGB { get; set; }
        public string? RetentionTime { get; set; }
    }
}
