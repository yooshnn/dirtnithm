using Microsoft.Extensions.Hosting;
using System.Windows;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Dirtnithm.App.Services;
using Dirtnithm.App.Infrastructure.Logging;

namespace Dirtnithm.App;

public partial class App : Application
{
    private IHost _host = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        LoggingConfig.Configure();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(services =>
            {
                services.AddTransient<MainWindow>();
                services.AddSingleton<SettingsService>();
                services.AddSingleton<IKeyInputService, KeyInputService>();
                services.AddSingleton<PipeService>();
                services.AddSingleton<ProcessService>();
                services.AddSingleton<HandCoordinatorService>();
            })
            .Build();

        var window = _host.Services.GetRequiredService<MainWindow>();
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
