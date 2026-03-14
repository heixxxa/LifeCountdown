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

        return new DashboardSnapshot(
            BuildLifeMetric(now, settings),
            BuildTimeMetric("本年", now, yearStart, yearStart.AddYears(1), $"{now:yyyy} 年"),
            BuildTimeMetric("本月", now, monthStart, monthStart.AddMonths(1), $"{now:yyyy 年 M 月}"),
            BuildTimeMetric("本周", now, weekStart, weekStart.AddDays(7), $"始于 {weekStart:MM/dd}"));
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

    private static MetricSnapshot BuildTimeMetric(string title, DateTime now, DateTime start, DateTime end, string caption)
    {
        var (percentage, elapsed, remaining) = CalculateWindow(now, start, end);

        return new MetricSnapshot(
            title,
            percentage,
            caption,
            $"已过 {elapsed.TotalDays:0.0} 天，还剩 {remaining.TotalDays:0.0} 天");
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
}
