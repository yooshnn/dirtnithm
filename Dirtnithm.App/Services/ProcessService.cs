using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Dirtnithm.App.Services;

public class ProcessService : IDisposable
{
    private const int MaxRestarts = 3;
    private const int RestartDelayMs = 1000;

    private readonly ILogger<ProcessService> _logger;
    private Process? _process;
    private string _exePath = string.Empty;
    private int _restartCount = 0;

    public event Action? OnMaxRestartsReached;

    public ProcessService(ILogger<ProcessService> logger) => _logger = logger;

    public void Start(string exePath)
    {
        _exePath = exePath;
        _restartCount = 0;
        Launch();
    }

    private void Launch()
    {
        _process?.Dispose();

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _exePath,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true,
        };

        _process.Exited += (_, _) => HandleProcessExited();
        _process.Start();
        _logger.LogInformation("Process started: {Path}", _exePath);
    }

    private void HandleProcessExited()
    {
        _logger.LogWarning("Process exited (restart {Count}/{Max})", _restartCount, MaxRestarts);

        if (_restartCount >= MaxRestarts)
        {
            _logger.LogError("Restart limit reached, giving up");
            OnMaxRestartsReached?.Invoke();
            return;
        }

        _restartCount++;
        Thread.Sleep(RestartDelayMs);
        Launch();
    }

    public void Dispose()
    {
        try
        {
            _process?.Kill();
            _logger.LogInformation("Process terminated");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to terminate process");
        }

        _process?.Dispose();
    }
}
