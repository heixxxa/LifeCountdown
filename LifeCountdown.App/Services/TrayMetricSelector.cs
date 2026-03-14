using LifeCountdown.App.Models;

namespace LifeCountdown.App.Services;

public static class TrayMetricSelector
{
    public static MetricSnapshot SelectMetric(DashboardSnapshot snapshot, TrayIconMetricMode mode)
    {
        return mode switch
        {
            TrayIconMetricMode.Life => snapshot.Life,
            TrayIconMetricMode.Year => snapshot.Year,
            TrayIconMetricMode.Month => snapshot.Month,
            TrayIconMetricMode.Week => snapshot.Week,
            TrayIconMetricMode.Day => snapshot.Day,
            TrayIconMetricMode.CustomCountdown => snapshot.EventMetrics.FirstOrDefault() ?? new MetricSnapshot("事件", 0, "未配置事件", "请在设置里添加自定义事件。"),
            _ => snapshot.Week,
        };
    }
}
