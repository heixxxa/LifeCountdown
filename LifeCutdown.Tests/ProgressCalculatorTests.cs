using LifeCutdown.App.Models;
using LifeCutdown.App.Services;

namespace LifeCutdown.Tests;

public class ProgressCalculatorTests
{
    [Fact]
    public void DayProgress_ReturnsMidpointAtNoon()
    {
        var settings = new AppSettings();

        var snapshot = ProgressCalculator.BuildDashboard(new DateTime(2026, 3, 11, 12, 0, 0), settings);

        Assert.InRange(snapshot.Day.Percentage, 50.0, 50.1);
    }

    [Fact]
    public void WeekProgress_UsesMondayAsConfigured()
    {
        var settings = new AppSettings
        {
            WeekStartMode = WeekStartMode.Monday,
        };

        var snapshot = ProgressCalculator.BuildDashboard(new DateTime(2026, 3, 11, 12, 0, 0), settings);

        Assert.InRange(snapshot.Week.Percentage, 35.7, 35.8);
    }

    [Fact]
    public void WeekProgress_UsesSundayAsConfigured()
    {
        var settings = new AppSettings
        {
            WeekStartMode = WeekStartMode.Sunday,
        };

        var snapshot = ProgressCalculator.BuildDashboard(new DateTime(2026, 3, 11, 12, 0, 0), settings);

        Assert.InRange(snapshot.Week.Percentage, 50.0, 50.1);
    }

    [Fact]
    public void LifeProgress_ReturnsExactMidpointForSymmetricRange()
    {
        var settings = new AppSettings
        {
            BirthDate = new DateTime(2001, 3, 1),
            LifeExpectancyYears = 40,
        };

        var snapshot = ProgressCalculator.BuildDashboard(new DateTime(2021, 3, 1, 0, 0, 0), settings);

        Assert.Equal(50.0, Math.Round(snapshot.Life.Percentage, 1));
    }

    [Fact]
    public void LifeProgress_ClampsToHundredAfterEndDate()
    {
        var settings = new AppSettings
        {
            BirthDate = new DateTime(2000, 1, 1),
            LifeExpectancyYears = 20,
        };

        var snapshot = ProgressCalculator.BuildDashboard(new DateTime(2025, 1, 1, 0, 0, 0), settings);

        Assert.Equal(100, snapshot.Life.Percentage);
        Assert.Contains("超出预设", snapshot.Life.Detail);
    }

    [Fact]
    public void EventMetrics_ReturnConfiguredEvents()
    {
        var settings = new AppSettings
        {
            CustomEvents = new List<CustomEventSettings>
            {
                new()
                {
                    Title = "项目截止",
                    StartDate = new DateTime(2026, 3, 1),
                    TargetDate = new DateTime(2026, 3, 11),
                },
                new()
                {
                    Title = "旅行",
                    StartDate = new DateTime(2026, 3, 5),
                    TargetDate = new DateTime(2026, 3, 25),
                },
            },
        };

        var snapshot = ProgressCalculator.BuildDashboard(new DateTime(2026, 3, 6, 0, 0, 0), settings);

        Assert.Equal(2, snapshot.EventMetrics.Count);
        Assert.Equal("项目截止", snapshot.EventMetrics[0].Title);
        Assert.InRange(snapshot.EventMetrics[0].Percentage, 50.0, 50.1);
        Assert.Equal("旅行", snapshot.EventMetrics[1].Title);
        Assert.InRange(snapshot.EventMetrics[1].Percentage, 5.0, 5.1);
    }

    [Fact]
    public void EventMetrics_FallsBackToLegacyCustomCountdown()
    {
        var settings = new AppSettings
        {
            CustomCountdownEnabled = true,
            CustomCountdownTitle = "项目截止",
            CustomCountdownStartDate = new DateTime(2026, 3, 1),
            CustomCountdownTargetDate = new DateTime(2026, 3, 11),
        };

        var snapshot = ProgressCalculator.BuildDashboard(new DateTime(2026, 3, 6, 0, 0, 0), settings);

        Assert.Single(snapshot.EventMetrics);
        Assert.Equal("项目截止", snapshot.EventMetrics[0].Title);
        Assert.InRange(snapshot.EventMetrics[0].Percentage, 50.0, 50.1);
    }

    [Fact]
    public void EventMetrics_ReturnsEmptyWhenDisabled()
    {
        var settings = new AppSettings
        {
            CustomCountdownEnabled = false,
        };

        var snapshot = ProgressCalculator.BuildDashboard(new DateTime(2026, 3, 6, 0, 0, 0), settings);

        Assert.Empty(snapshot.EventMetrics);
    }

    [Fact]
    public void TrayMetricSelector_ReturnsSelectedDayMetric()
    {
        var snapshot = ProgressCalculator.BuildDashboard(new DateTime(2026, 3, 11, 12, 0, 0), new AppSettings());

        var selected = TrayMetricSelector.SelectMetric(snapshot, TrayIconMetricMode.Day);

        Assert.Equal(snapshot.Day.Title, selected.Title);
        Assert.Equal(snapshot.Day.Percentage, selected.Percentage);
    }

    [Fact]
    public void TrayMetricSelector_ReturnsSelectedCustomMetric()
    {
        var settings = new AppSettings
        {
            CustomEvents = new List<CustomEventSettings>
            {
                new()
                {
                    Title = "发版",
                    StartDate = new DateTime(2026, 3, 1),
                    TargetDate = new DateTime(2026, 3, 20),
                },
            },
        };

        var snapshot = ProgressCalculator.BuildDashboard(new DateTime(2026, 3, 10, 0, 0, 0), settings);
        var selected = TrayMetricSelector.SelectMetric(snapshot, TrayIconMetricMode.CustomCountdown);

        Assert.Equal("发版", selected.Title);
        Assert.Equal(snapshot.EventMetrics[0].Percentage, selected.Percentage);
    }

    [Fact]
    public void TrayMetricSelector_ReturnsPlaceholderWhenNoEventsExist()
    {
        var snapshot = ProgressCalculator.BuildDashboard(new DateTime(2026, 3, 10, 0, 0, 0), new AppSettings());

        var selected = TrayMetricSelector.SelectMetric(snapshot, TrayIconMetricMode.CustomCountdown);

        Assert.Equal("事件", selected.Title);
        Assert.Equal(0, selected.Percentage);
        Assert.Contains("未配置事件", selected.Caption);
    }
}
