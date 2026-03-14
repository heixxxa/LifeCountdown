using System.Runtime.InteropServices;
using LifeCutdown.App.Models;
using Drawing = System.Drawing;

namespace LifeCutdown.App.Services;

public sealed class TaskbarWindowController : IDisposable
{
    private readonly Action _showWidget;
    private TaskbarMiniWindow? _window;
    private string? _lastWindowText;
    private string? _lastWindowDescription;
    private TrayIconMetricMode? _lastWindowMetricMode;
    private AppSettings? _lastSettings;
    private MetricSnapshot? _lastMetric;
    private IntPtr _attachedTaskbarHandle;
    private int _attachRetryTicks;

    public TaskbarWindowController(Action showWidget)
    {
        _showWidget = showWidget;
    }

    public void Update(AppSettings settings, DateTime now)
    {
        _lastSettings = settings.Clone();

        if (!settings.TaskbarDisplayEnabled)
        {
            HideWindow();
            return;
        }

        var snapshot = ProgressCalculator.BuildDashboard(now, settings);
        var metric = TrayMetricSelector.SelectMetric(snapshot, settings.TaskbarMetric);
        _lastMetric = metric;

        var window = EnsureWindow();
        var wasVisible = window.IsVisible;
        var windowText = MetricTextFormatter.BuildTaskbarWindowText(metric);
        var windowDescription = MetricTextFormatter.BuildTaskbarDescription(metric);
        var requiresMetricRefresh = !wasVisible
            || _lastWindowMetricMode != settings.TaskbarMetric
            || !string.Equals(_lastWindowText, windowText, StringComparison.Ordinal)
            || !string.Equals(_lastWindowDescription, windowDescription, StringComparison.Ordinal);

        if (requiresMetricRefresh)
        {
            window.UpdateMetric(metric, settings.TaskbarMetric);
            _lastWindowText = windowText;
            _lastWindowDescription = windowDescription;
            _lastWindowMetricMode = settings.TaskbarMetric;
        }

        RefreshLayout();
    }

    public void RefreshLayout()
    {
        if (_lastSettings is null || !_lastSettings.TaskbarDisplayEnabled || _lastMetric is null)
        {
            HideWindow();
            return;
        }

        if (!TryGetTaskbarAnchors(out var anchors) || !TaskbarWindowPlacementCalculator.SupportsTaskbarWindow(anchors.TaskbarBounds))
        {
            HideWindow();
            return;
        }

        var window = EnsureWindow();
        var attached = EnsureAttachment(window, anchors.TaskbarHandle);

        window.EnsureVisible();

        var placement = TaskbarWindowPlacementCalculator.Calculate(
            anchors.TaskbarBounds,
            anchors.TrayBounds,
            window.GetWindowPixelSize());

        window.ApplyPlacement(
            placement,
            attached ? anchors.TaskbarBounds.Location : null);
    }

    public void Dispose()
    {
        if (_window is null)
        {
            return;
        }

        _window.WidgetRequested -= Window_WidgetRequested;
        _window.Close();
        _window = null;
    }

    private TaskbarMiniWindow EnsureWindow()
    {
        if (_window is not null)
        {
            return _window;
        }

        _window = new TaskbarMiniWindow();
        _window.WidgetRequested += Window_WidgetRequested;
        return _window;
    }

    private void HideWindow()
    {
        if (_window?.IsVisible == true)
        {
            _window.Hide();
        }

        _attachedTaskbarHandle = IntPtr.Zero;
        _attachRetryTicks = 0;
    }

    private void Window_WidgetRequested(object? sender, EventArgs e)
    {
        _showWidget();
    }

    private bool EnsureAttachment(TaskbarMiniWindow window, IntPtr taskbarHandle)
    {
        if (taskbarHandle == IntPtr.Zero)
        {
            window.UseTopLevelFallback();
            _attachedTaskbarHandle = IntPtr.Zero;
            return false;
        }

        if (_attachedTaskbarHandle == taskbarHandle && window.IsAttachedToTaskbar)
        {
            return true;
        }

        if (_attachRetryTicks > 0 && _attachedTaskbarHandle == IntPtr.Zero)
        {
            _attachRetryTicks--;
            window.UseTopLevelFallback();
            return false;
        }

        if (window.TryAttachToTaskbar(taskbarHandle))
        {
            _attachedTaskbarHandle = taskbarHandle;
            _attachRetryTicks = 0;
            return true;
        }

        _attachedTaskbarHandle = IntPtr.Zero;
        _attachRetryTicks = 20;
        window.UseTopLevelFallback();
        return false;
    }

    private static bool TryGetTaskbarAnchors(out TaskbarAnchors anchors)
    {
        anchors = default;

        var taskbarHandle = FindWindow("Shell_TrayWnd", null);
        if (taskbarHandle == IntPtr.Zero || !GetWindowRect(taskbarHandle, out var taskbarRect))
        {
            return false;
        }

        var taskbarBounds = taskbarRect.ToDrawingRectangle();
        Drawing.Rectangle? trayBounds = null;
        Drawing.Rectangle? startBounds = null;

        var trayHandle = FindWindowEx(taskbarHandle, IntPtr.Zero, "TrayNotifyWnd", null);
        if (trayHandle != IntPtr.Zero && GetWindowRect(trayHandle, out var trayRect))
        {
            trayBounds = trayRect.ToDrawingRectangle();
        }

        var startHandle = FindWindowEx(taskbarHandle, IntPtr.Zero, "Start", null);
        if (startHandle != IntPtr.Zero && GetWindowRect(startHandle, out var startRect))
        {
            startBounds = startRect.ToDrawingRectangle();
        }

        anchors = new TaskbarAnchors(taskbarHandle, taskbarBounds, trayBounds, startBounds);
        return !taskbarBounds.IsEmpty;
    }

    private readonly record struct TaskbarAnchors(
        IntPtr TaskbarHandle,
        Drawing.Rectangle TaskbarBounds,
        Drawing.Rectangle? TrayBounds,
        Drawing.Rectangle? StartBounds);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr FindWindow(string? className, string? windowName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string? className, string? windowTitle);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect rectangle);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Rect
    {
        public int Left { get; init; }

        public int Top { get; init; }

        public int Right { get; init; }

        public int Bottom { get; init; }

        public Drawing.Rectangle ToDrawingRectangle()
        {
            return Drawing.Rectangle.FromLTRB(Left, Top, Right, Bottom);
        }
    }
}
