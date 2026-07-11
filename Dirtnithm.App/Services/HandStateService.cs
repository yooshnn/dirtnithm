using Microsoft.Extensions.Logging;

namespace Dirtnithm.App.Services;

public enum HandState
{
    Idle, Pressed, Releasing
}

public enum HandSide
{
    Left, Right
}

public class HandStateService
{
    private const double DefaultThresholdRatio = 0.5;
    private const int DefaultReleaseDelayMs = 300;

    private readonly ILogger<HandStateService> _logger;
    private readonly IKeyInputService _keyInput;
    private readonly HandSide _handSide;

    private HandState _state = HandState.Idle;
    private DateTime _releasingStart = DateTime.MinValue;

    public ushort VirtualKey { get; set; }
    public double ThresholdRatio { get; set; } = DefaultThresholdRatio;
    public int ReleaseDelayMs { get; set; } = DefaultReleaseDelayMs;
    public HandState State => _state;

    private bool HasReleaseDelayElapsed =>
        (DateTime.UtcNow - _releasingStart).TotalMilliseconds >= ReleaseDelayMs;

    public HandStateService(
        HandSide side,
        ushort virtualKey,
        IKeyInputService keyInput,
        ILogger<HandStateService> logger)
    {
        _handSide = side;
        VirtualKey = virtualKey;
        _keyInput = keyInput;
        _logger = logger;
    }

    public void Update(double? y)
    {
        bool isUp = y.HasValue && y.Value < ThresholdRatio;

        switch (_state)
        {
            case HandState.Idle:
                HandleIdle(isUp);
                break;
            case HandState.Pressed:
                HandlePressed(isUp);
                break;
            case HandState.Releasing:
                HandleReleasing(isUp);
                break;
        }
    }

    private void HandleIdle(bool isUp)
    {
        if (!isUp) return;

        _state = HandState.Pressed;
        _keyInput.Press(VirtualKey);
        _logger.LogDebug("[{Side}] Idle → Pressed", _handSide);
    }

    private void HandlePressed(bool isUp)
    {
        if (isUp) return;

        _state = HandState.Releasing;
        _releasingStart = DateTime.UtcNow;
        _logger.LogDebug("[{Side}] Pressed → Releasing", _handSide);
    }

    private void HandleReleasing(bool isUp)
    {
        if (isUp)
        {
            _state = HandState.Pressed;
            _logger.LogDebug("[{Side}] Releasing → Pressed", _handSide);
            return;
        }

        if (HasReleaseDelayElapsed)
        {
            _state = HandState.Idle;
            _keyInput.Release(VirtualKey);
            _logger.LogDebug("[{Side}] Releasing → Idle", _handSide);
        }
    }
}
