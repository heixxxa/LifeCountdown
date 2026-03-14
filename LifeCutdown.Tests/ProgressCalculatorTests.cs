using LifeCutdown.App.Models;
using LifeCutdown.App.Services;

namespace LifeCutdown.Tests;

public class ProgressCalculatorTests
{
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
}
