using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconnectDashboardGateway.Contracts.Enums
{
    // this is used to identify which dashboard the request is coming from  so that we can apply different auth logic if needed
    // and to identify the site type based on the site type in the site table .
    public enum SiteType
    {
        ChildDashboard = 1,
        ParentDashboard = 2,
    }
}
