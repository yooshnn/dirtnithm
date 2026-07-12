using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace Dirtnithm.App.Services;

public class TrayService : IDisposable
{
    private readonly ILogger<TrayService> _logger;
    private NotifyIcon _trayIcon = null!;

    public event Action? OnShowRequested;
    public event Action? OnQuitRequested;

    public TrayService(ILogger<TrayService> logger) => _logger = logger;

    public void Initialize(Icon icon)
    {
        _trayIcon = new NotifyIcon
        {
            Icon = icon,
            Text = "Dirtnithm",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };
        _trayIcon.DoubleClick += (_, _) => OnShowRequested?.Invoke();
    }

    public void SetStatus(bool connected)
        => _trayIcon.Text = connected ? "Dirtnithm - 연결됨" : "Dirtnithm - 대기";

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("열기", null, (_, _) => OnShowRequested?.Invoke());
        menu.Items.Add("종료", null, (_, _) => OnQuitRequested?.Invoke());
        return menu;
    }

    public void Dispose() => _trayIcon?.Dispose();
}