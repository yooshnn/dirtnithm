using System.IO;
using Serilog;

namespace Dirtnithm.App.Infrastructure.Logging;

public static class LoggingConfig
{
    private static readonly string _logPath = Path.Combine(
        AppContext.BaseDirectory, "logs", "dirtnithm.log");

    public static void Configure()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(_logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
