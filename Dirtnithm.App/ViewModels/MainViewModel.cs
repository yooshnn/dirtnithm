using System.Windows;
using System.Windows.Input;
using System.IO;
using Dirtnithm.App.Mvvm;
using Dirtnithm.App.Services;
using Microsoft.Extensions.Logging;

namespace Dirtnithm.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly PipeService _pipeService;
    private readonly ProcessService _processService;
    private readonly HandCoordinatorService _coordinator;

    public SettingsViewModel Settings { get; }
    public PreviewViewModel Preview { get; }

    public MainViewModel(
        ILogger<MainViewModel> logger,
        PipeService pipeService,
        ProcessService processService,
        HandCoordinatorService coordinator,
        SettingsViewModel settings,
        PreviewViewModel preview)
    {
        _logger = logger;
        _pipeService = pipeService;
        _processService = processService;
        _coordinator = coordinator;
        Settings = settings;
        Preview = preview;

        HideCommand = new RelayCommand(() => HideRequested?.Invoke());
        QuitCommand = new RelayCommand(() => QuitRequested?.Invoke());
    }

    private string _pipeStatus = "Python 연결 대기 중...";
    public string PipeStatus { get => _pipeStatus; set => SetProperty(ref _pipeStatus, value); }

    private string _leftState = "왼손: IDLE";
    public string LeftState { get => _leftState; set => SetProperty(ref _leftState, value); }

    private string _rightState = "오른손: IDLE";
    public string RightState { get => _rightState; set => SetProperty(ref _rightState, value); }

    public ICommand HideCommand { get; }
    public ICommand QuitCommand { get; }

    // The ViewModel must not know about Window types, so it only requests
    // hide/quit via events; MainWindow (the View) performs the actual action.
    public event Action? HideRequested;
    public event Action? QuitRequested;

    public async Task InitializeAsync()
    {
        Settings.LoadAndApply();
        TryStartVisionProcess();

        _pipeService.CoordinatesReceived += HandleCoordinatesReceived;

        await _pipeService.StartAsync();
    }

    private void TryStartVisionProcess()
    {
        var visionPath = Path.Combine(AppContext.BaseDirectory, "dirtnithm_vision.exe");
        if (!File.Exists(visionPath))
        {
            _logger.LogWarning("dirtnithm_vision.exe not found, please run Python manually");
            return;
        }

        _processService.OnMaxRestartsReached += HandleVisionProcessFailed;
        _processService.Start(visionPath);
    }

    private void HandleVisionProcessFailed() =>
        Application.Current.Dispatcher.Invoke(() =>
            MessageBox.Show("Python 프로세스가 반복 실패했습니다.", "오류"));

    private void HandleCoordinatesReceived(double? left, double? right)
    {
        // Marshal from the background thread PipeService raises this on,
        // to the UI thread (equivalent to the original Control.Invoke).
        Application.Current.Dispatcher.Invoke(() =>
        {
            PipeStatus = "Python 연결됨";
            LeftState = $"왼손: {_coordinator.LeftState}";
            RightState = $"오른손: {_coordinator.RightState}";
            Preview.UpdateFromCoordinates(left, right);
        });
    }
}
