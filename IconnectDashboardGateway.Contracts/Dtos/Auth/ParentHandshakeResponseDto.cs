using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconnectDashboardGateway.Contracts.Dtos.Auth
{
    public class ParentHandshakeResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresUtc { get; set; }
    }
}
