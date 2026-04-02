using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Application.Interfaces.Connection;
using IconnectDashboardGateway.Application.Interfaces.Repositories;

namespace IconnectDashboardGateway.Infrastructure.DataAccess
{
    public class CameraRepository:ICameraRepository
    {
        private readonly IRegistryConnectionStringProvider _connectionStringProvider;
        public CameraRepository(IRegistryConnectionStringProvider registryConnectionStringProvider)
        {
            _connectionStringProvider = registryConnectionStringProvider;
        }
        
    }
}
