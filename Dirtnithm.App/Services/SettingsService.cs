using System.IO;
using System.Text.Json;
using Dirtnithm.App.Models;

namespace Dirtnithm.App.Services;

public class SettingsService
{
    private static readonly string _settingsPath = Path.Combine(
        AppContext.BaseDirectory,
        "settings.json");

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public void Save(Settings settings)
    {
        var json = JsonSerializer.Serialize(settings, _options);
        File.WriteAllText(_settingsPath, json);
    }

    public Settings Load()
    {
        if (!File.Exists(_settingsPath))
        {
            return new Settings();
        }

        var json = File.ReadAllText(_settingsPath);
        return JsonSerializer.Deserialize<Settings>(json, _options) ?? new Settings();
    }
}
