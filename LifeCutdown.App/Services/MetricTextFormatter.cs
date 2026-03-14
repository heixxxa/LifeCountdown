using LifeCutdown.App.Models;

namespace LifeCutdown.App.Services;

public static class MetricTextFormatter
{
    public static string BuildTrayText(MetricSnapshot metric)
    {
        return TrimToLength($"{NormalizeTitle(metric.Title)} {metric.Percentage:0.0}%", 63);
    }

    public static string BuildTaskbarWindowText(MetricSnapshot metric)
    {
        return $"{TrimToLength(NormalizeTitle(metric.Title), 12)} {metric.Percentage:0.0}%";
    }

    public static string BuildTaskbarDescription(MetricSnapshot metric)
    {
        var parts = new[]
        {
            $"{NormalizeTitle(metric.Title)} {metric.Percentage:0.0}%",
            metric.Caption?.Trim(),
            metric.Detail?.Trim(),
        };

        return TrimToLength(
            string.Join(" | ", parts.Where(part => !string.IsNullOrWhiteSpace(part))),
            240);
    }

    private static string NormalizeTitle(string? title)
    {
        return string.IsNullOrWhiteSpace(title) ? "进度" : title.Trim();
    }

    private static string TrimToLength(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        return $"{text[..Math.Max(1, maxLength - 3)]}...";
    }
}
