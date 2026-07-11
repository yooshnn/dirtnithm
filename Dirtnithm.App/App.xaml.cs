using Dirtnithm.App.Infrastructure.Logging;
using Dirtnithm.App.Services;
using Dirtnithm.App.ViewModels;
using Dirtnithm.App.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Windows;

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
                services.AddTransient<MainViewModel>();
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
