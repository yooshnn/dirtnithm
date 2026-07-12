using System.Drawing;
using System.Windows;
using Dirtnithm.App.Services;
using Dirtnithm.App.ViewModels;

namespace Dirtnithm.App.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly TrayService _trayService;

    public MainWindow(MainViewModel vm, TrayService trayService)
    {
        InitializeComponent();
        _vm = vm;
        _trayService = trayService;
        DataContext = vm;

        _vm.HideRequested += HandleHideRequested;
        _vm.QuitRequested += HandleQuitRequested;

        _trayService.Initialize(GetIcon());
        _trayService.OnShowRequested += HandleTrayShowRequested;
        _trayService.OnQuitRequested += HandleQuitRequested;

        Loaded += HandleLoaded;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
        base.OnClosing(e);
    }

    private void HandleHideRequested() => Hide();

    private void HandleQuitRequested() => Dispatcher.Invoke(() => Application.Current.Shutdown());

    private void HandleTrayShowRequested() => Dispatcher.Invoke(() =>
    {
        Show();
        WindowState = WindowState.Normal;
    });

    private async void HandleLoaded(object sender, RoutedEventArgs e) => await _vm.InitializeAsync();

    private Icon GetIcon()
    {
        try
        {
            using var stream = GetType().Assembly
                .GetManifestResourceStream("Dirtnithm.App.Resources.app.ico");

            return stream is not null ? new Icon(stream) : SystemIcons.Application;
        }
        catch (ArgumentException)
        {
            return SystemIcons.Application;
        }
    }
}
