using Dirtnithm.App.Models;
using Microsoft.Extensions.Logging;

namespace Dirtnithm.App.Services;

public class HandCoordinatorService
{
    private readonly ILogger<HandCoordinatorService> _logger;
    private readonly ILogger<HandStateService> _handLogger;
    private readonly IKeyInputService _keyInputService;
    private readonly PipeService _pipeService;

    private Settings _currentSettings = new();
    private HandStateService? _leftHand;
    private HandStateService? _rightHand;

    public HandCoordinatorService(
        ILogger<HandCoordinatorService> logger,
        ILogger<HandStateService> handLogger,
        IKeyInputService keyInputService,
        PipeService pipeService)
    {
        _logger = logger;
        _handLogger = handLogger;
        _keyInputService = keyInputService;
        _pipeService = pipeService;
    }

    public void Apply(Settings settings)
    {
        _currentSettings = settings;
        _leftHand = new HandStateService(HandSide.Left, GetVirtualKey(settings.LeftKey),
            _keyInputService, _handLogger)
        {
            ThresholdRatio = settings.ThresholdPercent / 100.0,
            ReleaseDelayMs = settings.ReleaseDelayMs,
        };

        _rightHand = new HandStateService(HandSide.Right, GetVirtualKey(settings.RightKey),
            _keyInputService, _handLogger)
        {
            ThresholdRatio = settings.ThresholdPercent / 100.0,
            ReleaseDelayMs = settings.ReleaseDelayMs,
        };

        _pipeService.CoordinatesReceived -= HandleCoordinatesReceived;
        _pipeService.CoordinatesReceived += HandleCoordinatesReceived;

        _logger.LogInformation("Settings Applied - Left: {Left}, Right: {Right}",
            settings.LeftKey, settings.RightKey);
    }

    private void HandleCoordinatesReceived(double? left, double? right)
    {
        if (_currentSettings.IsMirrorEnabled)
        {
            (left, right) = (right, left);
        }

        _leftHand?.Update(left);
        _rightHand?.Update(right);
    }

    public HandState LeftState => _leftHand?.State ?? HandState.Idle;
    public HandState RightState => _rightHand?.State ?? HandState.Idle;

    private static ushort GetVirtualKey(string keyName) => keyName switch
    {
        "Space" => 0x20,
        "Return" => 0x0D,
        "Left" => 0x25,
        "Right" => 0x27,
        "Up" => 0x26,
        "Down" => 0x28,
        _ => (ushort)keyName[0]
    };
}