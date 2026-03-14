using System.Globalization;
using LifeCutdown.App.Helpers;
using LifeCutdown.App.Models;
using LifeCutdown.App.Services;

namespace LifeCutdown.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("zh-CN");
    private AppSettings _settings;
    private string _nowText = string.Empty;
    private string _footerText = string.Empty;

    public MainViewModel(AppSettings settings)
    {
        _settings = settings.Clone();

        LifeMetric = new ProgressMetric("一生");
        YearMetric = new ProgressMetric("本年");
        MonthMetric = new ProgressMetric("本月");
        WeekMetric = new ProgressMetric("本周");
        DayMetric = new ProgressMetric("本天");
        CustomCountdownMetric = new ProgressMetric("自定义倒计时");

        Refresh(DateTime.Now);
    }

    public ProgressMetric LifeMetric { get; }

    public ProgressMetric YearMetric { get; }

    public ProgressMetric MonthMetric { get; }

    public ProgressMetric WeekMetric { get; }

    public ProgressMetric DayMetric { get; }

    public ProgressMetric CustomCountdownMetric { get; }

    public string NowText
    {
        get => _nowText;
        private set => SetProperty(ref _nowText, value);
    }

    public string FooterText
    {
        get => _footerText;
        private set => SetProperty(ref _footerText, value);
    }

    public void UpdateSettings(AppSettings settings)
    {
        _settings = settings.Clone();
        Refresh(DateTime.Now);
    }

    public void Refresh(DateTime now)
    {
        var snapshot = ProgressCalculator.BuildDashboard(now, _settings);

        Apply(LifeMetric, snapshot.Life);
        Apply(YearMetric, snapshot.Year);
        Apply(MonthMetric, snapshot.Month);
        Apply(WeekMetric, snapshot.Week);
        Apply(DayMetric, snapshot.Day);
        Apply(CustomCountdownMetric, snapshot.CustomCountdown);

        NowText = now.ToString("yyyy 年 M 月 d 日 dddd HH:mm:ss", _culture);
        FooterText = _settings.WindowAnchor == WindowAnchor.BottomRight
            ? "窗口固定在右下角，靠近托盘；“托盘图标”按钮可展开或收起系统隐藏图标面板。"
            : "窗口固定在右上角；“托盘图标”按钮可展开或收起系统隐藏图标面板。";
    }

    private static void Apply(ProgressMetric target, MetricSnapshot snapshot)
    {
        target.Title = snapshot.Title;
        target.Percentage = snapshot.Percentage;
        target.Caption = snapshot.Caption;
        target.Detail = snapshot.Detail;
    }
}
