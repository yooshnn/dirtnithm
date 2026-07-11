namespace Dirtnithm.App.Models;

public class Settings
{
    public const string DefaultLeftKey = "A";
    public const string DefaultRightKey = "B";
    public const int DefaultThresholdPercent = 50;
    public const int DefaultReleaseDelayMs = 100;
    public const bool DefaultIsMirrorEnabled = false;

    public string LeftKey { get; set; } = DefaultLeftKey;
    public string RightKey { get; set; } = DefaultRightKey;
    public int ThresholdPercent { get; set; } = DefaultThresholdPercent;
    public int ReleaseDelayMs { get; set; } = DefaultReleaseDelayMs;
    public bool IsMirrorEnabled { get; set; } = DefaultIsMirrorEnabled;
}
