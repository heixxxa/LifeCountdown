namespace LifeCutdown.App.Models;

public sealed class AppSettings
{
    public DateTime BirthDate { get; set; } = new(1995, 1, 1);

    public int LifeExpectancyYears { get; set; } = 85;

    public WeekStartMode WeekStartMode { get; set; } = WeekStartMode.Monday;

    public WindowAnchor WindowAnchor { get; set; } = WindowAnchor.BottomRight;

    public AppSettings Clone()
    {
        return new AppSettings
        {
            BirthDate = BirthDate,
            LifeExpectancyYears = LifeExpectancyYears,
            WeekStartMode = WeekStartMode,
            WindowAnchor = WindowAnchor,
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
