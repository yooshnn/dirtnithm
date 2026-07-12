using Microsoft.Extensions.Logging;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace Dirtnithm.App.Services;

public class PipeService : IDisposable
{
    private const string CoordPipeName = "dirtnithm_coords";
    private readonly ILogger<PipeService> _logger;
    private NamedPipeServerStream? _coordPipe;
    private StreamReader? _reader;
    private CancellationTokenSource? _cts;
    private Task? _listenTask;

    public event Action<double?, double?>? CoordinatesReceived;

    public PipeService(ILogger<PipeService> logger) => _logger = logger;

    public Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        _listenTask = Task.Run(() => CoordListenLoopAsync(_cts.Token));
        return _listenTask;
    }

    private async Task CoordListenLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await AcceptAndListenAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CoordPipe error; retrying in 1s");
                await Task.Delay(1000, ct);
            }
            finally
            {
                _reader?.Dispose();
                _coordPipe?.Dispose();
                _reader = null;
                _coordPipe = null;
            }
        }
    }

    private async Task AcceptAndListenAsync(CancellationToken ct)
    {
        _coordPipe = new NamedPipeServerStream(
            CoordPipeName,
            PipeDirection.In,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);

        _logger.LogInformation("Waiting for Python connection");
        await _coordPipe.WaitForConnectionAsync(ct);
        _logger.LogInformation("Python connected");

        _reader = new StreamReader(_coordPipe, Encoding.UTF8);

        while (!ct.IsCancellationRequested && _coordPipe.IsConnected)
        {
            var line = await _reader.ReadLineAsync(ct);
            if (line == null) break;
            ParseAndNotify(line);
        }

        _logger.LogInformation("Python disconnected");
    }

    private void ParseAndNotify(string line)
    {
        try
        {
            var doc = JsonDocument.Parse(line);
            double? left = ReadNullableDouble(doc.RootElement, "left");
            double? right = ReadNullableDouble(doc.RootElement, "right");
            CoordinatesReceived?.Invoke(left, right);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse coordinates: {Line}", line);
        }
    }

    private static double? ReadNullableDouble(JsonElement root, string propertyName)
    {
        var element = root.GetProperty(propertyName);
        return element.ValueKind == JsonValueKind.Null ? null : element.GetDouble();
    }

    public void Dispose()
    {
        _cts?.Cancel();

        try
        {
            _listenTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // Safe to ignore:
            // Expected when cancellation triggers task completion
        }

        _cts?.Dispose();
    }
}
