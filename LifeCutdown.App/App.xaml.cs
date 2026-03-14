using System.ComponentModel;
using System.Windows.Threading;
using LifeCutdown.App.Models;
using LifeCutdown.App.Services;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace LifeCutdown.App;

public partial class App : System.Windows.Application
{
    private SettingsService? _settingsService;
    private AppSettings? _settings;
    private Forms.NotifyIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private DispatcherTimer? _trayRefreshTimer;
    private Drawing.Icon? _defaultTrayIcon;
    private Drawing.Icon? _generatedTrayIcon;
    private bool _isExitRequested;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        _settingsService = new SettingsService();
        var isFirstLaunch = !_settingsService.Exists;
        _settings = _settingsService.Load();

        _mainWindow = new MainWindow(_settingsService, _settings, isFirstLaunch);
        _mainWindow.Closing += MainWindow_Closing;
        _mainWindow.SettingsSaved += MainWindow_SettingsSaved;

        InitializeTrayIcon();
        InitializeTrayTimer();
        _mainWindow.ShowWidget();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        if (_trayRefreshTimer is not null)
        {
            _trayRefreshTimer.Stop();
            _trayRefreshTimer = null;
        }

        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }

        _generatedTrayIcon?.Dispose();
        _generatedTrayIcon = null;

        _defaultTrayIcon?.Dispose();
        _defaultTrayIcon = null;

        base.OnExit(e);
    }

    private void InitializeTrayIcon()
    {
        _defaultTrayIcon = (Drawing.Icon)Drawing.SystemIcons.Application.Clone();

        _trayIcon = new Forms.NotifyIcon
        {
            Text = "人生进度条",
            Icon = _defaultTrayIcon,
            Visible = true,
        };

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("显示 / 隐藏", null, (_, _) => ToggleWindow());
        menu.Items.Add("设置", null, (_, _) => OpenSettings());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => ExitApplication());

        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => ToggleWindow();

        UpdateTrayIcon();
    }

    private void InitializeTrayTimer()
    {
        _trayRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _trayRefreshTimer.Tick += (_, _) => UpdateTrayIcon();
        _trayRefreshTimer.Start();
    }

    private void ToggleWindow()
    {
        Dispatcher.Invoke(() =>
        {
            if (_mainWindow is null)
            {
                return;
            }

            if (_mainWindow.IsVisible)
            {
                _mainWindow.Hide();
                return;
            }

            _mainWindow.ShowWidget();
        });
    }

    private void OpenSettings()
    {
        Dispatcher.Invoke(() =>
        {
            if (_mainWindow is null)
            {
                return;
            }

            _mainWindow.ShowWidget();
            _mainWindow.OpenSettingsDialog();
        });
    }

    private void ExitApplication()
    {
        Dispatcher.Invoke(() =>
        {
            _isExitRequested = true;

            if (_trayIcon is not null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }

            if (_mainWindow is not null)
            {
                _mainWindow.Close();
                _mainWindow = null;
            }

            Shutdown();
        });
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_isExitRequested)
        {
            return;
        }

        e.Cancel = true;
        _mainWindow?.Hide();
    }

    private void MainWindow_SettingsSaved(object? sender, AppSettings settings)
    {
        _settings = settings.Clone();
        UpdateTrayIcon();
    }

    private void UpdateTrayIcon()
    {
        if (_trayIcon is null || _settings is null)
        {
            return;
        }

        if (_settings.TrayIconMetric == TrayIconMetricMode.DefaultIcon)
        {
            _trayIcon.Icon = _defaultTrayIcon;
            _trayIcon.Text = "人生进度条";
            _generatedTrayIcon?.Dispose();
            _generatedTrayIcon = null;
            return;
        }

        var snapshot = ProgressCalculator.BuildDashboard(DateTime.Now, _settings);
        var metric = TrayMetricSelector.SelectMetric(snapshot, _settings.TrayIconMetric);
        var nextIcon = TrayIconRenderer.CreateProgressIcon(metric, _settings.TrayIconMetric);
        var previousIcon = _generatedTrayIcon;

        _generatedTrayIcon = nextIcon;
        _trayIcon.Icon = _generatedTrayIcon;
        _trayIcon.Text = TrayIconRenderer.BuildTrayText(metric);

        previousIcon?.Dispose();
    }
}
