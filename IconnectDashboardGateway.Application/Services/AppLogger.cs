
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IconnectDashboardGateway.Application.Enums;
using IconnectDashboardGateway.Application.Interfaces;

namespace IconnectDashboardGateway.Application.Services
{
    public class AppLogger:IAppLogger
    {
        private readonly string _logsDirectory;
        private readonly object _sync = new();
        public AppLogger()
        {
            _logsDirectory = Path.Combine(Directory.GetCurrentDirectory()) ?? throw new ArgumentNullException(nameof(_logsDirectory));
        }
        public void Log(AppLogLevel level, string message, Exception? exception = null)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
            var sb = new StringBuilder();
            sb.Append(DateTimeOffset.Now.ToString("O"));
            sb.Append(" | ");
            sb.Append(level.ToString().ToUpperInvariant());
            sb.Append(" | ");
            sb.AppendLine(message);
            if (exception != null)
                sb.AppendLine(exception.ToString());
            sb.AppendLine();
            Append(sb.ToString());
        }
        private void Append(string text)
        {
            lock (_sync)
            {
                Directory.CreateDirectory(_logsDirectory);
                var path = Path.Combine(_logsDirectory, $"app-{DateTime.UtcNow:yyyy-MM-dd}.log");
                File.AppendAllText(path, text, Encoding.UTF8);
            }
        }
    }                                                                                                                                                                               
}
