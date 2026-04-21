using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Application.Enums;

namespace IconnectDashboardGateway.Application.Interfaces.Logger
{
    /// Application-level file logger for errors and exceptions
    public interface IAppLogger
    {
        void Log(AppLogLevel level, string message, Exception? exception = null);
        void LogInformation(string message) => Log(AppLogLevel.Information, message, null);
        void LogWarning(string message) => Log(AppLogLevel.Warning, message, null);
        void LogError(string message, Exception? exception = null) => Log(AppLogLevel.Error, message, exception);
    }
}
