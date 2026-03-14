using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LifeCountdown.App.Models;
using LifeCountdown.App.Services;
using LifeCountdown.App.ViewModels;

namespace LifeCountdown.App;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _refreshTimer;
    private readonly bool _showSettingsOnFirstLaunch;
    private AppSettings _settings;

    public event EventHandler<AppSettings>? SettingsSaved;

    public MainWindow(SettingsService settingsService, AppSettings settings, bool showSettingsOnFirstLaunch)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _settings = settings.Clone();
        _showSettingsOnFirstLaunch = showSettingsOnFirstLaunch;

        _viewModel = new MainViewModel(_settings);
        DataContext = _viewModel;

        _refreshTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _refreshTimer.Tick += RefreshTimer_Tick;

        _viewModel.Refresh(DateTime.Now);

        Loaded += MainWindow_Loaded;
    }

    public bool IsWidgetOpen => IsVisible;

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

    public void DismissWidget()
    {
        Hide();
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
        ApplySettings(_settings);
        SettingsSaved?.Invoke(this, _settings.Clone());
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

    private void ApplySettings(AppSettings settings)
    {
        _settings = settings.Clone();
        _viewModel.UpdateSettings(_settings);
        _viewModel.Refresh(DateTime.Now);
    }

    protected override void OnClosed(EventArgs e)
    {
        _refreshTimer.Stop();
        base.OnClosed(e);
    }
}
