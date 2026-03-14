using LifeCutdown.App.Models;

namespace LifeCutdown.App.Services;

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
            TrayIconMetricMode.CustomCountdown => snapshot.CustomCountdown,
            _ => snapshot.Week,
        };
    }
}
