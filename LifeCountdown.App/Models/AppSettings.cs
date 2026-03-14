namespace LifeCountdown.App.Models;

public sealed class AppSettings
{
    public DateTime BirthDate { get; set; } = new(1995, 1, 1);

    public int LifeExpectancyYears { get; set; } = 85;

    public WeekStartMode WeekStartMode { get; set; } = WeekStartMode.Monday;

    public WindowAnchor WindowAnchor { get; set; } = WindowAnchor.BottomRight;

    public TrayIconMetricMode TrayIconMetric { get; set; } = TrayIconMetricMode.Week;

    public bool TaskbarDisplayEnabled { get; set; }

    public TrayIconMetricMode TaskbarMetric { get; set; } = TrayIconMetricMode.Week;

    public bool CustomCountdownEnabled { get; set; }

    public string CustomCountdownTitle { get; set; } = "自定义倒计时";

    public DateTime CustomCountdownStartDate { get; set; } = DateTime.Today;

    public DateTime CustomCountdownTargetDate { get; set; } = DateTime.Today.AddDays(30);

    public List<CustomEventSettings> CustomEvents { get; set; } = new();

    public AppSettings Clone()
    {
        return new AppSettings
        {
            BirthDate = BirthDate,
            LifeExpectancyYears = LifeExpectancyYears,
            WeekStartMode = WeekStartMode,
            WindowAnchor = WindowAnchor,
            TrayIconMetric = TrayIconMetric,
            TaskbarDisplayEnabled = TaskbarDisplayEnabled,
            TaskbarMetric = TaskbarMetric,
            CustomCountdownEnabled = CustomCountdownEnabled,
            CustomCountdownTitle = CustomCountdownTitle,
            CustomCountdownStartDate = CustomCountdownStartDate,
            CustomCountdownTargetDate = CustomCountdownTargetDate,
            CustomEvents = CustomEvents.Select(customEvent => customEvent.Clone()).ToList(),
        };
    }
}

public enum WeekStartMode
{
    Monday,
    Sunday,
}

public enum WindowAnchor
{
    TopRight,
    BottomRight,
}

public enum TrayIconMetricMode
{
    DefaultIcon,
    Life,
    Year,
    Month,
    Week,
    Day,
    CustomCountdown,
}
