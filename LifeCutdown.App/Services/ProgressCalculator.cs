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
            BuildTimeMetric("本年", now, yearStart, yearStart.AddYears(1), $"{now:yyyy} 年"),
            BuildTimeMetric("本月", now, monthStart, monthStart.AddMonths(1), $"{now:yyyy 年 M 月}"),
            BuildTimeMetric("本周", now, weekStart, weekStart.AddDays(7), $"始于 {weekStart:MM/dd}"),
            BuildTimeMetric("本天", now, dayStart, dayStart.AddDays(1), $"{now:yyyy/MM/dd}", preferClockDetail: true),
            BuildCustomCountdownMetric(now, settings));
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

    public static MetricSnapshot BuildCustomCountdownMetric(DateTime now, AppSettings settings)
    {
        var title = string.IsNullOrWhiteSpace(settings.CustomCountdownTitle)
            ? "自定义倒计时"
            : settings.CustomCountdownTitle.Trim();

        if (!settings.CustomCountdownEnabled)
        {
            return new MetricSnapshot(
                title,
                0,
                "未启用自定义倒计时",
                "在设置中填写标题、开始日期和目标日期。"
            );
        }

        var startDate = settings.CustomCountdownStartDate.Date;
        var targetDate = settings.CustomCountdownTargetDate.Date;

        if (targetDate <= startDate)
        {
            targetDate = startDate.AddDays(1);
        }

        var (percentage, _, remaining) = CalculateWindow(now, startDate, targetDate);

        string detail;
        if (now < startDate)
        {
            detail = $"距开始还有 {FormatSpan(startDate - now)}";
        }
        else if (now < targetDate)
        {
            detail = $"距离目标还有 {FormatSpan(remaining)}";
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
