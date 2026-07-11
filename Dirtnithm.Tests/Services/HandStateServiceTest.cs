using Dirtnithm.App.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dirtnithm.Tests.Services;

public class HandStateServiceTest
{
    private readonly IKeyInputService _keyInput = Substitute.For<IKeyInputService>();
    private readonly ILogger<HandStateService> _logger = Substitute.For<ILogger<HandStateService>>();

    private HandStateService Create(ushort vk = 0x20, int releaseDelayMs = 300) =>
        new HandStateService(HandSide.Left, vk, _keyInput, _logger)
        {
            ThresholdRatio = 0.5,
            ReleaseDelayMs = releaseDelayMs
        };

    [Fact]
    public void Idle_WhenHandAboveThreshold_TransitionsToPressed()
    {
        var svc = Create();
        svc.Update(0.2);
        Assert.Equal(HandState.Pressed, svc.State);
    }

    [Fact]
    public void Idle_WhenHandAboveThreshold_CallsPressOnce()
    {
        var svc = Create(0x20);
        svc.Update(0.2);
        _keyInput.Received(1).Press(0x20);
    }

    [Fact]
    public void Pressed_WhenHandBelowThreshold_TransitionsToReleasing()
    {
        var svc = Create();
        svc.Update(0.2);
        svc.Update(0.8);
        Assert.Equal(HandState.Releasing, svc.State);
    }

    [Fact]
    public void Pressed_WhenHandLost_TransitionsToReleasing()
    {
        var svc = Create();
        svc.Update(0.2);
        svc.Update(null);
        Assert.Equal(HandState.Releasing, svc.State);
    }

    [Fact]
    public void Releasing_WhenHandReturnsAboveThreshold_TransitionsBackToPressed()
    {
        var svc = Create();
        svc.Update(0.2);
        svc.Update(0.8);
        svc.Update(0.2);
        Assert.Equal(HandState.Pressed, svc.State);
    }

    [Fact]
    public void Releasing_WhenDelayElapsed_TransitionsToIdle()
    {
        var svc = Create(releaseDelayMs: 0);
        svc.Update(0.2);
        svc.Update(0.8);
        svc.Update(0.8);
        Assert.Equal(HandState.Idle, svc.State);
    }

    [Fact]
    public void Releasing_WhenDelayElapsed_CallsReleaseOnce()
    {
        var svc = Create(0x20);
        svc.ReleaseDelayMs = 0;
        svc.Update(0.2);
        svc.Update(0.8);
        svc.Update(0.8);
        _keyInput.Received(1).Release(0x20);
    }
}
