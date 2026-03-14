using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using LifeCountdown.App.Models;
using LifeCountdown.App.Services;
using Drawing = System.Drawing;

namespace LifeCountdown.App;

public partial class TaskbarMiniWindow : Window
{
    private const int GwlStyle = -16;
    private const int GwlExStyle = -20;
    private const int WsChild = 0x40000000;
    private const int WsVisible = 0x10000000;
    private const int WsPopup = unchecked((int)0x80000000);
    private const int WsExToolWindow = 0x00000080;
    private const int WsExNoActivate = 0x08000000;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpFrameChanged = 0x0020;
    private const uint SwpShowWindow = 0x0040;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoOwnerZOrder = 0x0200;
    private static readonly IntPtr HwndTopMost = new(-1);

    private double _percentage;
    private IntPtr _parentHandle;

    public TaskbarMiniWindow()
    {
        InitializeComponent();

        Loaded += (_, _) => UpdateProgressBar();
        SizeChanged += (_, _) => UpdateProgressBar();
        SourceInitialized += TaskbarMiniWindow_SourceInitialized;
    }

    public event EventHandler? WidgetRequested;

    public bool IsAttachedToTaskbar => _parentHandle != IntPtr.Zero;

    public void UpdateMetric(MetricSnapshot metric, TrayIconMetricMode mode)
    {
        _percentage = metric.Percentage;
        MetricTitleTextBlock.Text = MetricTextFormatter.BuildTaskbarWindowTitle(metric);
        MetricPercentageTextBlock.Text = MetricTextFormatter.BuildTaskbarWindowPercentage(metric);
        RootBorder.ToolTip = MetricTextFormatter.BuildTaskbarDescription(metric);

        var accentColor = GetMetricColor(mode);
        var accentBrush = new SolidColorBrush(accentColor);
        accentBrush.Freeze();
        
        var bgBrush = new SolidColorBrush(accentColor) { Opacity = 0.25 };
        bgBrush.Freeze();

        MetricTitleTextBlock.Foreground = accentBrush;
        MetricPercentageTextBlock.Foreground = accentBrush;
        ProgressFillRectangle.Fill = accentBrush;
        ProgressBackgroundRectangle.Fill = bgBrush;

        UpdateLayout();
        UpdateProgressBar();
    }

    public Drawing.Size GetWindowPixelSize()
    {
        UpdateLayout();

        var dpi = VisualTreeHelper.GetDpi(this);
        return new Drawing.Size(
            Math.Max(1, (int)Math.Ceiling(ActualWidth * dpi.DpiScaleX)),
            Math.Max(1, (int)Math.Ceiling(ActualHeight * dpi.DpiScaleY)));
    }

    public void EnsureVisible()
    {
        if (!IsVisible)
        {
            Show();
        }
    }

    public bool TryAttachToTaskbar(IntPtr parentHandle)
    {
        var handle = EnsureWindowHandle();
        if (handle == IntPtr.Zero || parentHandle == IntPtr.Zero)
        {
            return false;
        }

        if (_parentHandle == parentHandle && GetParent(handle) == parentHandle)
        {
            return true;
        }

        var style = GetWindowLong(handle, GwlStyle);
        SetWindowLong(handle, GwlStyle, (style | WsChild | WsVisible) & ~WsPopup);
        SetParent(handle, parentHandle);
        ApplyFrameChange(handle);

        if (GetParent(handle) == parentHandle)
        {
            _parentHandle = parentHandle;
            return true;
        }

        UseTopLevelFallback();
        return false;
    }

    public void UseTopLevelFallback()
    {
        var handle = EnsureWindowHandle();
        if (handle == IntPtr.Zero)
        {
            return;
        }

        SetParent(handle, IntPtr.Zero);

        var style = GetWindowLong(handle, GwlStyle);
        SetWindowLong(handle, GwlStyle, (style | WsPopup | WsVisible) & ~WsChild);
        ApplyFrameChange(handle);
        _parentHandle = IntPtr.Zero;
    }

    public void ApplyPlacement(Drawing.Rectangle bounds, Drawing.Point? parentScreenOrigin)
    {
        var handle = EnsureWindowHandle();
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var x = bounds.Left;
        var y = bounds.Top;
        var flags = SwpNoActivate | SwpShowWindow;
        var insertAfter = HwndTopMost;

        if (parentScreenOrigin is Drawing.Point origin && _parentHandle != IntPtr.Zero)
        {
            x -= origin.X;
            y -= origin.Y;
            flags |= SwpNoZOrder | SwpNoOwnerZOrder;
            insertAfter = IntPtr.Zero;
        }

        SetWindowPos(
            handle,
            insertAfter,
            x,
            y,
            bounds.Width,
            bounds.Height,
            flags);
    }

    private void TaskbarMiniWindow_SourceInitialized(object? sender, EventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var exStyle = GetWindowLong(handle, GwlExStyle);
        SetWindowLong(handle, GwlExStyle, exStyle | WsExToolWindow | WsExNoActivate);
    }

    private void RootBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        WidgetRequested?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateProgressBar()
    {
        var width = ProgressTrackGrid.ActualWidth;
        ProgressFillRectangle.Width = width * Math.Clamp(_percentage, 0, 100) / 100.0;
    }

    private static System.Windows.Media.Color GetMetricColor(TrayIconMetricMode mode)
    {
        return mode switch
        {
            TrayIconMetricMode.Life => System.Windows.Media.Color.FromRgb(116, 136, 158),
            TrayIconMetricMode.Year => System.Windows.Media.Color.FromRgb(124, 157, 129),
            TrayIconMetricMode.Month => System.Windows.Media.Color.FromRgb(170, 134, 88),
            TrayIconMetricMode.Week => System.Windows.Media.Color.FromRgb(138, 146, 156),
            TrayIconMetricMode.Day => System.Windows.Media.Color.FromRgb(145, 121, 169),
            TrayIconMetricMode.CustomCountdown => System.Windows.Media.Color.FromRgb(183, 113, 106),
            _ => System.Windows.Media.Color.FromRgb(138, 146, 156),
        };
    }

    private static void ApplyFrameChange(IntPtr handle)
    {
        SetWindowPos(
            handle,
            IntPtr.Zero,
            0,
            0,
            0,
            0,
            SwpNoActivate | SwpNoMove | SwpNoSize | SwpNoZOrder | SwpNoOwnerZOrder | SwpFrameChanged);
    }

    private IntPtr EnsureWindowHandle()
    {
        var helper = new WindowInteropHelper(this);
        return helper.EnsureHandle();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int index);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int index, int newLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
}