using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconnectDashboardGateway.Application.Interfaces.Connection
{
    public interface IRegistryConnectionStringProvider
    {
        /// Returns the decrypted SQL connection string from registery (cached, refreshed on interval).
       public string GetConnectionString();
    }
}
