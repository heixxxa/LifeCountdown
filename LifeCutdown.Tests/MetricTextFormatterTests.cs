using LifeCutdown.App.Models;
using LifeCutdown.App.Services;

namespace LifeCutdown.Tests;

public class MetricTextFormatterTests
{
    [Fact]
    public void BuildTaskbarWindowText_IncludesMetricNameAndPercentage()
    {
        var metric = new MetricSnapshot("本周", 42.34, "第 11 周", "已过 2.9 天");

        var title = MetricTextFormatter.BuildTaskbarWindowText(metric);

        Assert.Equal("本周 42.3%", title);
    }

    [Fact]
    public void BuildTaskbarWindowText_TrimsLongMetricNames()
    {
        var metric = new MetricSnapshot("这是一个非常非常非常长的里程碑事件名称", 88.88, "caption", "detail");

        var title = MetricTextFormatter.BuildTaskbarWindowText(metric);

        Assert.StartsWith("这是一个非常非常非...", title);
        Assert.Contains("88.9%", title);
    }

    [Fact]
    public void BuildTrayText_RespectsNotifyIconLimit()
    {
        var metric = new MetricSnapshot("这是一个非常非常非常长的里程碑事件名称并且还会继续增长直到超过通知区域限制", 12.34, "caption", "detail");

        var text = MetricTextFormatter.BuildTrayText(metric);

        Assert.True(text.Length <= 63);
        Assert.Contains("12.3%", text);
    }
}
