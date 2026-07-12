using Dirtnithm.App.Models;
using Dirtnithm.App.Mvvm;
using Dirtnithm.App.Services;

namespace Dirtnithm.App.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly HandCoordinatorService _coordinator;
    private readonly PreviewViewModel _preview;

    private Settings _settings = new();

    // Guards against Save() firing while LoadAndApply() is still populating properties.
    private bool _isLoaded;

    public SettingsViewModel(
        SettingsService settingsService,
        HandCoordinatorService coordinator,
        PreviewViewModel preview)
    {
        _settingsService = settingsService;
        _coordinator = coordinator;
        _preview = preview;
    }

    private string _leftKey = Settings.DefaultLeftKey;
    public string LeftKey
    {
        get => _leftKey;
        set { if (SetProperty(ref _leftKey, value)) Save(); }
    }

    private string _rightKey = Settings.DefaultRightKey;
    public string RightKey
    {
        get => _rightKey;
        set { if (SetProperty(ref _rightKey, value)) Save(); }
    }

    private int _thresholdPercent = Settings.DefaultThresholdPercent;
    public int ThresholdPercent
    {
        get => _thresholdPercent;
        set
        {
            if (SetProperty(ref _thresholdPercent, value))
            {
                _preview.ThresholdPercent = value;
                Save();
            }
        }
    }

    private int _releaseDelayMs = Settings.DefaultReleaseDelayMs;
    public int ReleaseDelayMs
    {
        get => _releaseDelayMs;
        set { if (SetProperty(ref _releaseDelayMs, value)) Save(); }
    }

    private bool _isMirrorEnabled = Settings.DefaultIsMirrorEnabled;
    public bool IsMirrorEnabled
    {
        get => _isMirrorEnabled;
        set { if (SetProperty(ref _isMirrorEnabled, value)) Save(); }
    }

    // Loads persisted settings into these properties and applies them to the
    // coordinator. MainViewModel.InitializeAsync must call this *before*
    // subscribing to PipeService.CoordinatesReceived, so the coordinator's
    // handler (registered inside Apply) updates hand state before anything
    // else reads it.
    public void LoadAndApply()
    {
        _settings = _settingsService.Load();

        LeftKey = _settings.LeftKey;
        RightKey = _settings.RightKey;
        ThresholdPercent = _settings.ThresholdPercent;
        ReleaseDelayMs = _settings.ReleaseDelayMs;
        IsMirrorEnabled = _settings.IsMirrorEnabled;

        _coordinator.Apply(_settings);
        _isLoaded = true;
    }

    private void Save()
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
