using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Civilium
{
    public static class Logger
    {
        private static readonly ILogger _logger;

        static Logger()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }

        public static void LogInformation(string message)
        {
            _logger.Information(message);
        }

        public static void LogWarning(string message, Exception ex = null)
        {
            if (ex == null)
                _logger.Warning(message);
            else
                _logger.Warning(ex, message);
        }

        public static void LogError(string message, Exception ex = null)
        {
            if (ex == null)
                _logger.Error(message);
            else
                _logger.Error(ex, message);
        }

        public static void LogDebug(string message)
        {
            _logger.Debug(message);
        }
    }
}
