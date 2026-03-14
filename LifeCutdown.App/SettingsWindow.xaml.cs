using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using LifeCutdown.App.Models;
using LifeCutdown.App.Services;

namespace LifeCutdown.App;

public partial class SettingsWindow : Window
{
    private readonly ObservableCollection<CustomEventDraft> _eventDrafts = new();
    private readonly SystemTrayController _systemTrayController = new();

    private sealed record SelectionItem<T>(string Label, T Value)
    {
        public override string ToString()
        {
            return Label;
        }
    }

    private sealed class CustomEventDraft
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");

        public string Title { get; set; } = "自定义事件";

        public DateTime? StartDate { get; set; } = DateTime.Today;

        public DateTime? TargetDate { get; set; } = DateTime.Today.AddDays(30);
    }

    public SettingsWindow(AppSettings currentSettings)
    {
        InitializeComponent();

        Result = currentSettings.Clone();

        WeekStartComboBox.ItemsSource = new[]
        {
            new SelectionItem<WeekStartMode>("周一开始", WeekStartMode.Monday),
            new SelectionItem<WeekStartMode>("周日开始", WeekStartMode.Sunday),
        };

        WindowAnchorComboBox.ItemsSource = new[]
        {
            new SelectionItem<WindowAnchor>("右下角（靠近托盘）", WindowAnchor.BottomRight),
            new SelectionItem<WindowAnchor>("右上角", WindowAnchor.TopRight),
        };

        TrayIconMetricComboBox.ItemsSource = new[]
        {
            new SelectionItem<TrayIconMetricMode>("默认应用图标", TrayIconMetricMode.DefaultIcon),
            new SelectionItem<TrayIconMetricMode>("一生", TrayIconMetricMode.Life),
            new SelectionItem<TrayIconMetricMode>("本年", TrayIconMetricMode.Year),
            new SelectionItem<TrayIconMetricMode>("本月", TrayIconMetricMode.Month),
            new SelectionItem<TrayIconMetricMode>("本周", TrayIconMetricMode.Week),
            new SelectionItem<TrayIconMetricMode>("本天", TrayIconMetricMode.Day),
            new SelectionItem<TrayIconMetricMode>("首个事件", TrayIconMetricMode.CustomCountdown),
        };

        BirthDatePicker.SelectedDate = Result.BirthDate;
        LifeExpectancyTextBox.Text = Result.LifeExpectancyYears.ToString(CultureInfo.InvariantCulture);
        WeekStartComboBox.SelectedIndex = Result.WeekStartMode == WeekStartMode.Monday ? 0 : 1;
        WindowAnchorComboBox.SelectedIndex = Result.WindowAnchor == WindowAnchor.BottomRight ? 0 : 1;
        TrayIconMetricComboBox.SelectedIndex = Result.TrayIconMetric switch
        {
            TrayIconMetricMode.DefaultIcon => 0,
            TrayIconMetricMode.Life => 1,
            TrayIconMetricMode.Year => 2,
            TrayIconMetricMode.Month => 3,
            TrayIconMetricMode.Week => 4,
            TrayIconMetricMode.Day => 5,
            TrayIconMetricMode.CustomCountdown => 6,
            _ => 4,
        };

        CustomEventsItemsControl.ItemsSource = _eventDrafts;

        foreach (var customEvent in GetSeedEvents(Result))
        {
            _eventDrafts.Add(new CustomEventDraft
            {
                Id = string.IsNullOrWhiteSpace(customEvent.Id) ? Guid.NewGuid().ToString("N") : customEvent.Id,
                Title = customEvent.Title,
                StartDate = customEvent.StartDate,
                TargetDate = customEvent.TargetDate,
            });
        }
    }

    public AppSettings Result { get; private set; }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (BirthDatePicker.SelectedDate is not DateTime birthDate)
        {
            System.Windows.MessageBox.Show(this, "请先选择出生日期。", "无法保存", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(LifeExpectancyTextBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var lifeExpectancy) || lifeExpectancy is < 1 or > 130)
        {
            System.Windows.MessageBox.Show(this, "预期寿命请输入 1 到 130 之间的整数。", "无法保存", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (birthDate.Date > DateTime.Today)
        {
            System.Windows.MessageBox.Show(this, "出生日期不能晚于今天。", "无法保存", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var weekStartSelection = WeekStartComboBox.SelectedItem as SelectionItem<WeekStartMode>;
        var anchorSelection = WindowAnchorComboBox.SelectedItem as SelectionItem<WindowAnchor>;
        var trayMetricSelection = TrayIconMetricComboBox.SelectedItem as SelectionItem<TrayIconMetricMode>;

        if (weekStartSelection is null || anchorSelection is null || trayMetricSelection is null)
        {
            System.Windows.MessageBox.Show(this, "请完成基础设置项。", "无法保存", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var customEvents = new List<CustomEventSettings>();

        foreach (var draft in _eventDrafts)
        {
            var title = string.IsNullOrWhiteSpace(draft.Title) ? "自定义事件" : draft.Title.Trim();
            var startDate = draft.StartDate ?? DateTime.Today;
            var targetDate = draft.TargetDate ?? startDate.AddDays(30);

            if (targetDate <= startDate)
            {
                System.Windows.MessageBox.Show(this, $"事件“{title}”的目标日期必须晚于开始日期。", "无法保存", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            customEvents.Add(new CustomEventSettings
            {
                Id = draft.Id,
                Title = title,
                StartDate = startDate.Date,
                TargetDate = targetDate.Date,
            });
        }

        var primaryEvent = customEvents.FirstOrDefault();

        Result = new AppSettings
        {
            BirthDate = birthDate.Date,
            LifeExpectancyYears = lifeExpectancy,
            WeekStartMode = weekStartSelection.Value,
            WindowAnchor = anchorSelection.Value,
            TrayIconMetric = trayMetricSelection.Value,
            CustomEvents = customEvents,
            CustomCountdownEnabled = primaryEvent is not null,
            CustomCountdownTitle = primaryEvent?.Title ?? "自定义倒计时",
            CustomCountdownStartDate = primaryEvent?.StartDate ?? DateTime.Today,
            CustomCountdownTargetDate = primaryEvent?.TargetDate ?? DateTime.Today.AddDays(30),
        };

        DialogResult = true;
    }

    private void AddCustomEventButton_Click(object sender, RoutedEventArgs e)
    {
        _eventDrafts.Add(new CustomEventDraft
        {
            Title = $"事件 {_eventDrafts.Count + 1}",
            StartDate = DateTime.Today,
            TargetDate = DateTime.Today.AddDays(30),
        });
    }

    private void RemoveCustomEventButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: CustomEventDraft draft })
        {
            return;
        }

        _eventDrafts.Remove(draft);
    }

    private void OpenSystemTraySettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (_systemTrayController.TryOpenTraySettings())
        {
            return;
        }

        System.Windows.MessageBox.Show(
            this,
            "未能打开 Windows 的托盘设置页。",
            "系统托盘设置",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private static IEnumerable<CustomEventSettings> GetSeedEvents(AppSettings settings)
    {
        if (settings.CustomEvents.Count > 0)
        {
            return settings.CustomEvents;
        }

        if (!settings.CustomCountdownEnabled)
        {
            return Enumerable.Empty<CustomEventSettings>();
        }

        return new[]
        {
            new CustomEventSettings
            {
                Title = settings.CustomCountdownTitle,
                StartDate = settings.CustomCountdownStartDate,
                TargetDate = settings.CustomCountdownTargetDate,
            }
        };
    }
}
