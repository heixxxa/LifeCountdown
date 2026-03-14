using System.IO;
using System.Text.Json;
using LifeCutdown.App.Models;

namespace LifeCutdown.App.Services;

public sealed class SettingsService
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string _settingsPath;

    public SettingsService()
    {
        var settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LifeCutdown");

        _settingsPath = Path.Combine(settingsDirectory, "settings.json");
    }

    public bool Exists => File.Exists(_settingsPath);

    public AppSettings Load()
    {
        try
        {
            if (Exists)
            {
                var json = File.ReadAllText(_settingsPath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json, _serializerOptions);

                if (loaded is not null)
                {
                    return Normalize(loaded);
                }
            }
        }
        catch
        {
        }

        var defaults = Normalize(new AppSettings());
        Save(defaults);
        return defaults;
    }

    public void Save(AppSettings settings)
    {
        var normalized = Normalize(settings);
        var directory = Path.GetDirectoryName(_settingsPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(normalized, _serializerOptions);
        File.WriteAllText(_settingsPath, json);
    }

    private static AppSettings Normalize(AppSettings settings)
    {
        var normalized = settings.Clone();

        if (normalized.BirthDate == default)
        {
            normalized.BirthDate = new DateTime(1995, 1, 1);
        }

        normalized.BirthDate = normalized.BirthDate.Date;
        normalized.LifeExpectancyYears = Math.Clamp(normalized.LifeExpectancyYears, 1, 130);

        if (string.IsNullOrWhiteSpace(normalized.CustomCountdownTitle))
        {
            normalized.CustomCountdownTitle = "自定义倒计时";
        }

        normalized.CustomCountdownStartDate = normalized.CustomCountdownStartDate == default
            ? DateTime.Today
            : normalized.CustomCountdownStartDate.Date;

        normalized.CustomCountdownTargetDate = normalized.CustomCountdownTargetDate == default
            ? normalized.CustomCountdownStartDate.AddDays(30)
            : normalized.CustomCountdownTargetDate.Date;

        if (normalized.CustomCountdownTargetDate <= normalized.CustomCountdownStartDate)
        {
            normalized.CustomCountdownTargetDate = normalized.CustomCountdownStartDate.AddDays(1);
        }

        return normalized;
    }
}
