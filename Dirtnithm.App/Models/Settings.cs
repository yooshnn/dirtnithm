namespace Dirtnithm.App.Models;

public class Settings
{
    public string LeftKey { get; set; } = "A";
    public string RightKey { get; set; } = "B";
    public int ThresholdPercent { get; set; } = 50;
    public int ReleaseDelayMs { get; set; } = 100;
    public bool IsMirrorEnabled { get; set; } = false;
}
