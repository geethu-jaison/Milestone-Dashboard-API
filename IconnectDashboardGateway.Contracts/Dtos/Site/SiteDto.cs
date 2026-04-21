using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconnectDashboardGateway.Contracts.Dtos.Site
{
    public class SiteDto
    {
        public string SiteId { get; set; }
        public int SiteType { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public string? SiteApiAddress { get; set; }
        public string? SiteDescription { get; set; }
        public string SiteData { get; set; } = string.Empty;
        public DateTime DateAddedUtc { get; set; }
        public DateTime DateModifiedUtc { get; set; }
    }
}
