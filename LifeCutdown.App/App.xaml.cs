using System.ComponentModel;
using LifeCutdown.App.Services;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace LifeCutdown.App;

public partial class App : System.Windows.Application
{
    private Forms.NotifyIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private bool _isExitRequested;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        var settingsService = new SettingsService();
        var isFirstLaunch = !settingsService.Exists;
        var settings = settingsService.Load();

        _mainWindow = new MainWindow(settingsService, settings, isFirstLaunch);
        _mainWindow.Closing += MainWindow_Closing;

        InitializeTrayIcon();
        _mainWindow.ShowWidget();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }

        base.OnExit(e);
    }

    private void InitializeTrayIcon()
    {
        _trayIcon = new Forms.NotifyIcon
        {
            Text = "人生进度条",
            Icon = Drawing.SystemIcons.Application,
            Visible = true,
        };

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("显示 / 隐藏", null, (_, _) => ToggleWindow());
        menu.Items.Add("设置", null, (_, _) => OpenSettings());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => ExitApplication());

        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => ToggleWindow();
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
}
