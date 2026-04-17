using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconnectDashboardGateway.Contracts.Dtos.Auth
{
    public class SiteAuthOptions
    {
        public const string SectionName = "SiteAuth";
        public bool Enabled { get; set; } = true;
        public string TimestampHeaderName { get; set; } = "X-Site-Timestamp";
        public string SignatureHeaderName { get; set; } = "X-Site-Signature";
        public string[] BypassPathPrefixes { get; set; } = ["/openapi", "/scalar"];
        public int MaxClockSkewSeconds { get; set; } = 300;
    }
}
