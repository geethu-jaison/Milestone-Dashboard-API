using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Application.Interfaces.Connection;
using IconnectDashboardGateway.Application.Interfaces.DataAccess;
using IconnectDashboardGateway.Application.Interfaces.Logger;

namespace IconnectDashboardGateway.Infrastructure.DataAccess
{

    [Obfuscation(ApplyToMembers =true,Exclude =false)]
    public class SiteRepository:ISiteRepository
    {
        private readonly IAppLogger _appLogger;
        private readonly IRegistryConnectionStringProvider _ConnectionStringProvider;
        public SiteRepository(IAppLogger appLogger,IRegistryConnectionStringProvider connectionStringProvider)
        {
            _appLogger = appLogger;
            _ConnectionStringProvider = connectionStringProvider;
        }
    }
}
