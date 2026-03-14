using System.Collections.ObjectModel;
using LifeCountdown.App.Helpers;
using LifeCountdown.App.Models;
using LifeCountdown.App.Services;

namespace LifeCountdown.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly ProgressMetric[] _timeMetricItems;
    private AppSettings _settings;
    private string _eventEmptyText = string.Empty;
    private bool _hasEventMetrics;

    public MainViewModel(AppSettings settings)
    {
        _settings = settings.Clone();

        _timeMetricItems = new[]
        {
            new ProgressMetric("一生"),
            new ProgressMetric("本年"),
            new ProgressMetric("本月"),
            new ProgressMetric("本周"),
            new ProgressMetric("本天"),
        };

        TimeMetrics = new ObservableCollection<ProgressMetric>(_timeMetricItems);
        EventMetrics = new ObservableCollection<ProgressMetric>();

        Refresh(DateTime.Now);
    }

    public ObservableCollection<ProgressMetric> TimeMetrics { get; }

    public ObservableCollection<ProgressMetric> EventMetrics { get; }

    public string EventEmptyText
    {
        get => _eventEmptyText;
        private set => SetProperty(ref _eventEmptyText, value);
    }

    public bool HasEventMetrics
    {
        get => _hasEventMetrics;
        private set
        {
            if (SetProperty(ref _hasEventMetrics, value))
            {
                OnPropertyChanged(nameof(ShowEventEmptyState));
            }
        }
    }

    public bool ShowEventEmptyState => !HasEventMetrics;

    public void UpdateSettings(AppSettings settings)
    {
        _settings = settings.Clone();
        Refresh(DateTime.Now);
    }

    public void Refresh(DateTime now)
    {
        var snapshot = ProgressCalculator.BuildDashboard(now, _settings);

        Apply(_timeMetricItems[0], snapshot.Life);
        Apply(_timeMetricItems[1], snapshot.Year);
        Apply(_timeMetricItems[2], snapshot.Month);
        Apply(_timeMetricItems[3], snapshot.Week);
        Apply(_timeMetricItems[4], snapshot.Day);

        SyncEventMetrics(snapshot.EventMetrics);
        HasEventMetrics = EventMetrics.Count > 0;
        EventEmptyText = EventMetrics.Count == 0
            ? "暂无自定义事件。请从托盘菜单打开设置进行添加。"
            : string.Empty;
    }

    private void SyncEventMetrics(IReadOnlyList<MetricSnapshot> snapshots)
    {
        while (EventMetrics.Count < snapshots.Count)
        {
            EventMetrics.Add(new ProgressMetric("事件"));
        }

        while (EventMetrics.Count > snapshots.Count)
        {
            EventMetrics.RemoveAt(EventMetrics.Count - 1);
        }

        for (var index = 0; index < snapshots.Count; index++)
        {
            Apply(EventMetrics[index], snapshots[index]);
        }
    }

    private static void Apply(ProgressMetric target, MetricSnapshot snapshot)
    {
        target.Title = snapshot.Title;
        target.Percentage = snapshot.Percentage;
        target.Caption = snapshot.Caption;
        target.Detail = snapshot.Detail;
    }
}
