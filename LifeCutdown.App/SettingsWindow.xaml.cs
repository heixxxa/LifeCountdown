using System.Globalization;
using System.Windows;
using System.Windows.Input;
using LifeCutdown.App.Models;

namespace LifeCutdown.App;

public partial class SettingsWindow : Window
{
    private sealed record SelectionItem<T>(string Label, T Value)
    {
        public override string ToString()
        {
            return Label;
        }
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

        BirthDatePicker.SelectedDate = Result.BirthDate;
        LifeExpectancyTextBox.Text = Result.LifeExpectancyYears.ToString(CultureInfo.InvariantCulture);
        WeekStartComboBox.SelectedIndex = Result.WeekStartMode == WeekStartMode.Monday ? 0 : 1;
        WindowAnchorComboBox.SelectedIndex = Result.WindowAnchor == WindowAnchor.BottomRight ? 0 : 1;
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

        if (weekStartSelection is null || anchorSelection is null)
        {
            System.Windows.MessageBox.Show(this, "请完成全部设置项。", "无法保存", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Result = new AppSettings
        {
            BirthDate = birthDate.Date,
            LifeExpectancyYears = lifeExpectancy,
            WeekStartMode = weekStartSelection.Value,
            WindowAnchor = anchorSelection.Value,
        };

        DialogResult = true;
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
}
