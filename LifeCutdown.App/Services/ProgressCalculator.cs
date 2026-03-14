using LifeCutdown.App.Models;

namespace LifeCutdown.App.Services;

public static class ProgressCalculator
{
    private const double DaysPerYear = 365.2425;

    public static DashboardSnapshot BuildDashboard(DateTime now, AppSettings settings)
    {
        var yearStart = new DateTime(now.Year, 1, 1);
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var weekStart = GetStartOfWeek(now, settings.WeekStartMode);
        var dayStart = now.Date;

        return new DashboardSnapshot(
            BuildLifeMetric(now, settings),
            BuildTimeMetric("本年", now, yearStart, yearStart.AddYears(1), $"{now:yyyy}年"),
            BuildTimeMetric("本月", now, monthStart, monthStart.AddMonths(1), $"{now:yyyy年M月}"),
            BuildTimeMetric("本周", now, weekStart, weekStart.AddDays(7), $"始于 {weekStart:MM/dd}"),
            BuildTimeMetric("本天", now, dayStart, dayStart.AddDays(1), $"{now:yyyy/MM/dd}", preferClockDetail: true),
            BuildEventMetrics(now, settings));
    }

    public static MetricSnapshot BuildLifeMetric(DateTime now, AppSettings settings)
    {
        var birthDate = settings.BirthDate.Date;
        var expectancy = Math.Clamp(settings.LifeExpectancyYears, 1, 130);
        var endDate = birthDate.AddYears(expectancy);

        var (percentage, elapsed, remaining) = CalculateWindow(now, birthDate, endDate);
        var detail = remaining > TimeSpan.Zero
            ? $"已过 {ToYears(elapsed):0.0} 年，还剩 {ToYears(remaining):0.0} 年"
            : $"已走过 {ToYears(endDate - birthDate):0.0} 年，超出预设 {ToYears(now - endDate):0.0} 年";

        return new MetricSnapshot(
            "一生",
            percentage,
            $"出生 {birthDate:yyyy/MM/dd} · 预期 {expectancy} 岁",
            detail);
    }

    public static IReadOnlyList<MetricSnapshot> BuildEventMetrics(DateTime now, AppSettings settings)
    {
        return GetCustomEvents(settings)
            .Select(customEvent => BuildEventMetric(now, customEvent))
            .ToList();
    }

    private static MetricSnapshot BuildEventMetric(DateTime now, CustomEventSettings customEvent)
    {
        var startDate = customEvent.StartDate.Date;
        var targetDate = customEvent.TargetDate.Date;
        var title = string.IsNullOrWhiteSpace(customEvent.Title) ? "自定义事件" : customEvent.Title.Trim();

        if (targetDate <= startDate)
        {
            targetDate = startDate.AddDays(1);
        }

        var (percentage, elapsed, remaining) = CalculateWindow(now, startDate, targetDate);
        string detail;

        if (now < startDate)
        {
            detail = $"距开始还有 {FormatSpan(startDate - now)}";
        }
        else if (now < targetDate)
        {
            detail = $"已过 {FormatSpan(elapsed)}，还剩 {FormatSpan(remaining)}";
        }
        else
        {
            detail = $"已超出目标 {FormatSpan(now - targetDate)}";
        }

        return new MetricSnapshot(
            title,
            percentage,
            $"起点 {startDate:yyyy/MM/dd} · 目标 {targetDate:yyyy/MM/dd}",
            detail);
    }

    private static MetricSnapshot BuildTimeMetric(string title, DateTime now, DateTime start, DateTime end, string caption, bool preferClockDetail = false)
    {
        var (percentage, elapsed, remaining) = CalculateWindow(now, start, end);

        return new MetricSnapshot(
            title,
            percentage,
            caption,
            $"已过 {FormatSpan(elapsed, preferClockDetail)}，还剩 {FormatSpan(remaining, preferClockDetail)}");
    }

    private static IEnumerable<CustomEventSettings> GetCustomEvents(AppSettings settings)
    {
        if (settings.CustomEvents.Count > 0)
        {
            return settings.CustomEvents;
        }

        if (!settings.CustomCountdownEnabled)
        {
            return Enumerable.Empty<CustomEventSettings>();
        }

        return new[]
        {
            new CustomEventSettings
            {
                Title = settings.CustomCountdownTitle,
                StartDate = settings.CustomCountdownStartDate,
                TargetDate = settings.CustomCountdownTargetDate,
            }
        };
    }

    private static (double Percentage, TimeSpan Elapsed, TimeSpan Remaining) CalculateWindow(DateTime now, DateTime start, DateTime end)
    {
        if (end <= start)
        {
            return (0, TimeSpan.Zero, TimeSpan.Zero);
        }

        if (now <= start)
        {
            return (0, TimeSpan.Zero, end - start);
        }

        if (now >= end)
        {
            return (100, end - start, TimeSpan.Zero);
        }

        var total = end - start;
        var elapsed = now - start;
        var remaining = end - now;
        var percentage = elapsed.TotalMilliseconds / total.TotalMilliseconds * 100.0;
        return (Math.Clamp(percentage, 0, 100), elapsed, remaining);
    }

    private static DateTime GetStartOfWeek(DateTime now, WeekStartMode weekStartMode)
    {
        var firstDay = weekStartMode == WeekStartMode.Monday ? DayOfWeek.Monday : DayOfWeek.Sunday;
        var delta = (7 + (int)now.DayOfWeek - (int)firstDay) % 7;
        return now.Date.AddDays(-delta);
    }

    private static double ToYears(TimeSpan span)
    {
        return span.TotalDays / DaysPerYear;
    }

    private static string FormatSpan(TimeSpan span, bool preferClockDetail = false)
    {
        if (span <= TimeSpan.Zero)
        {
            return "0 分钟";
        }

        if (preferClockDetail)
        {
            var totalHours = (int)Math.Floor(span.TotalHours);
            return totalHours > 0
                ? $"{totalHours} 小时 {span.Minutes} 分"
                : $"{span.Minutes} 分 {span.Seconds} 秒";
        }

        if (span.TotalDays >= 2)
        {
            return $"{span.TotalDays:0.0} 天";
        }

        if (span.TotalHours >= 1)
        {
            return $"{Math.Floor(span.TotalHours)} 小时 {span.Minutes} 分";
        }

        if (span.TotalMinutes >= 1)
        {
            return $"{Math.Floor(span.TotalMinutes)} 分 {span.Seconds} 秒";
        }

        return $"{Math.Max(1, Math.Floor(span.TotalSeconds))} 秒";
    }
}
