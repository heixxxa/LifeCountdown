using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LifeCutdown.App.Models;
using LifeCutdown.App.Services;
using LifeCutdown.App.ViewModels;

namespace LifeCutdown.App;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly SystemTrayController _systemTrayController;
    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _refreshTimer;
    private readonly bool _showSettingsOnFirstLaunch;
    private AppSettings _settings;

    public MainWindow(SettingsService settingsService, AppSettings settings, bool showSettingsOnFirstLaunch)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _systemTrayController = new SystemTrayController();
        _settings = settings.Clone();
        _showSettingsOnFirstLaunch = showSettingsOnFirstLaunch;

        _viewModel = new MainViewModel(_settings);
        DataContext = _viewModel;

        _refreshTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _refreshTimer.Tick += RefreshTimer_Tick;

        Loaded += MainWindow_Loaded;
    }

    public void ShowWidget()
    {
        if (!IsVisible)
        {
            Show();
        }

        WindowState = WindowState.Normal;
        PositionWindow();
        Activate();
    }

    public void OpenSettingsDialog()
    {
        var dialog = new SettingsWindow(_settings)
        {
            Owner = this,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _settings = dialog.Result.Clone();
        _settingsService.Save(_settings);
        _viewModel.UpdateSettings(_settings);
        PositionWindow();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _refreshTimer.Start();
        PositionWindow();

        if (_showSettingsOnFirstLaunch)
        {
            Dispatcher.BeginInvoke(OpenSettingsDialog, DispatcherPriority.ContextIdle);
        }
    }

    private void RefreshTimer_Tick(object? sender, EventArgs e)
    {
        _viewModel.Refresh(DateTime.Now);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        OpenSettingsDialog();
    }

    private void TrayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_systemTrayController.ToggleOverflowWindow())
        {
            return;
        }

        System.Windows.MessageBox.Show(
            this,
            "当前系统未能直接切换隐藏托盘图标面板。你可以在设置页里点击“打开系统托盘设置”来管理其他应用的托盘图标显示。",
            "托盘图标",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void HideButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void PositionWindow()
    {
        var workArea = SystemParameters.WorkArea;
        var width = ActualWidth > 0 ? ActualWidth : Width;
        var height = ActualHeight > 0 ? ActualHeight : Height;

        Left = workArea.Right - width - 20;
        Top = _settings.WindowAnchor == WindowAnchor.TopRight
            ? workArea.Top + 20
            : workArea.Bottom - height - 20;
    }

    protected override void OnClosed(EventArgs e)
    {
        _refreshTimer.Stop();
        base.OnClosed(e);
    }
}
