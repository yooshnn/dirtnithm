using System.Windows;
using System.Windows.Input;
using System.IO;
using Dirtnithm.App.Models;
using Dirtnithm.App.Mvvm;
using Dirtnithm.App.Services;
using Microsoft.Extensions.Logging;

namespace Dirtnithm.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly SettingsService _settingsService;
    private readonly PipeService _pipeService;
    private readonly ProcessService _processService;
    private readonly HandCoordinatorService _coordinator;

    private Settings _settings = new();
    public PreviewViewModel Preview { get; } = new();

    // Guards against SaveSettings firing while InitializeAsync is still populating properties.
    private bool _isLoaded;

    public MainViewModel(
        ILogger<MainViewModel> logger,
        SettingsService settingsService,
        PipeService pipeService,
        ProcessService processService,
        HandCoordinatorService coordinator)
    {
        _logger = logger;
        _settingsService = settingsService;
        _pipeService = pipeService;
        _processService = processService;
        _coordinator = coordinator;

        HideCommand = new RelayCommand(() => HideRequested?.Invoke());
        QuitCommand = new RelayCommand(() => QuitRequested?.Invoke());
    }

    private string _pipeStatus = "Python 연결 대기 중...";
    public string PipeStatus { get => _pipeStatus; set => SetProperty(ref _pipeStatus, value); }

    private string _leftState = "왼손: IDLE";
    public string LeftState { get => _leftState; set => SetProperty(ref _leftState, value); }

    private string _rightState = "오른손: IDLE";
    public string RightState { get => _rightState; set => SetProperty(ref _rightState, value); }

    private string _leftKey = Settings.DefaultLeftKey;
    public string LeftKey
    {
        get => _leftKey;
        set { if (SetProperty(ref _leftKey, value)) SaveSettings(); }
    }

    private string _rightKey = Settings.DefaultRightKey;
    public string RightKey
    {
        get => _rightKey;
        set { if (SetProperty(ref _rightKey, value)) SaveSettings(); }
    }

    private int _thresholdPercent = Settings.DefaultThresholdPercent;
    public int ThresholdPercent
    {
        get => _thresholdPercent;
        set
        {
            if (!SetProperty(ref _thresholdPercent, value)) return;
            Preview.ThresholdPercent = value;
            SaveSettings();
        }
    }

    private int _releaseDelayMs = Settings.DefaultReleaseDelayMs;
    public int ReleaseDelayMs
    {
        get => _releaseDelayMs;
        set { if (SetProperty(ref _releaseDelayMs, value)) SaveSettings(); }
    }

    private bool _isMirrorEnabled = Settings.DefaultIsMirrorEnabled;
    public bool IsMirrorEnabled
    {
        get => _isMirrorEnabled;
        set { if (SetProperty(ref _isMirrorEnabled, value)) SaveSettings(); }
    }

    public ICommand HideCommand { get; }
    public ICommand QuitCommand { get; }

    // The ViewModel must not know about Window types, so it only requests
    // hide/quit via events; MainWindow (the View) performs the actual action.
    public event Action? HideRequested;
    public event Action? QuitRequested;

    public async Task InitializeAsync()
    {
        _settings = _settingsService.Load();
        ApplySettingsToProperties(_settings);
        TryStartVisionProcess();

        // Apply() must run before we subscribe, so the coordinator's handler
        // (registered inside Apply) updates state before ours reads it —
        // otherwise this ViewModel would read one frame of stale state.
        _coordinator.Apply(_settings);
        _pipeService.CoordinatesReceived += HandleCoordinatesReceived;

        // Must be set before awaiting StartAsync below, since that loop
        // effectively never returns.
        _isLoaded = true;

        await _pipeService.StartAsync();
    }

    private void ApplySettingsToProperties(Settings settings)
    {
        LeftKey = settings.LeftKey;
        RightKey = settings.RightKey;
        ThresholdPercent = settings.ThresholdPercent;
        ReleaseDelayMs = settings.ReleaseDelayMs;
        IsMirrorEnabled = settings.IsMirrorEnabled;
    }

    private void TryStartVisionProcess()
    {
        var visionPath = Path.Combine(AppContext.BaseDirectory, "dirtnithm_vision.exe");
        if (!File.Exists(visionPath))
        {
            _logger.LogWarning("dirtnithm_vision.exe not found, please run Python manually");
            return;
        }

        _processService.OnMaxRestartsReached += () =>
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show("Python 프로세스가 반복 실패했습니다.", "오류"));
        _processService.Start(visionPath);
    }

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

    private void SaveSettings()
    {
        if (!_isLoaded) return;

        _settings.LeftKey = LeftKey;
        _settings.RightKey = RightKey;
        _settings.ThresholdPercent = ThresholdPercent;
        _settings.ReleaseDelayMs = ReleaseDelayMs;
        _settings.IsMirrorEnabled = IsMirrorEnabled;

        _settingsService.Save(_settings);
        _coordinator.Apply(_settings);
    }
}
