namespace LifeCutdown.App.Models;

public sealed record MetricSnapshot(string Title, double Percentage, string Caption, string Detail);

public sealed record DashboardSnapshot(
    MetricSnapshot Life,
    MetricSnapshot Year,
    MetricSnapshot Month,
    MetricSnapshot Week);
