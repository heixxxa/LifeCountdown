namespace LifeCutdown.App.Models;

public sealed class AppSettings
{
    public DateTime BirthDate { get; set; } = new(1995, 1, 1);

    public int LifeExpectancyYears { get; set; } = 85;

    public WeekStartMode WeekStartMode { get; set; } = WeekStartMode.Monday;

    public WindowAnchor WindowAnchor { get; set; } = WindowAnchor.BottomRight;

    public bool CustomCountdownEnabled { get; set; }

    public string CustomCountdownTitle { get; set; } = "自定义倒计时";

    public DateTime CustomCountdownStartDate { get; set; } = DateTime.Today;

    public DateTime CustomCountdownTargetDate { get; set; } = DateTime.Today.AddDays(30);

    public AppSettings Clone()
    {
        return new AppSettings
        {
            BirthDate = BirthDate,
            LifeExpectancyYears = LifeExpectancyYears,
            WeekStartMode = WeekStartMode,
            WindowAnchor = WindowAnchor,
            CustomCountdownEnabled = CustomCountdownEnabled,
            CustomCountdownTitle = CustomCountdownTitle,
            CustomCountdownStartDate = CustomCountdownStartDate,
            CustomCountdownTargetDate = CustomCountdownTargetDate,
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
