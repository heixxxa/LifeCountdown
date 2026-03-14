using Drawing = System.Drawing;

namespace LifeCountdown.App.Services;

public static class TaskbarWindowPlacementCalculator
{
    private const int DefaultHorizontalMargin = 6;
    private const int DefaultFallbackRightPadding = 88;

    public static bool SupportsTaskbarWindow(Drawing.Rectangle taskbarBounds)
    {
        return taskbarBounds.Width >= taskbarBounds.Height;
    }

    public static Drawing.Rectangle Calculate(
        Drawing.Rectangle taskbarBounds,
        Drawing.Rectangle? trayBounds,
        Drawing.Size windowSize,
        int horizontalMargin = DefaultHorizontalMargin,
        int fallbackRightPadding = DefaultFallbackRightPadding)
    {
        var trayLeft = trayBounds is { Width: > 0 } bounds && bounds.Left > taskbarBounds.Left
            ? bounds.Left
            : taskbarBounds.Right - fallbackRightPadding;

        var width = Math.Max(1, windowSize.Width);
        var height = Math.Max(1, windowSize.Height);
        var unclampedLeft = trayLeft - width - horizontalMargin;
        var left = Math.Clamp(
            unclampedLeft,
            taskbarBounds.Left + horizontalMargin,
            Math.Max(taskbarBounds.Left + horizontalMargin, taskbarBounds.Right - width - horizontalMargin));

        var unclampedTop = taskbarBounds.Top + Math.Max(0, (taskbarBounds.Height - height) / 2);
        var top = Math.Clamp(
            unclampedTop,
            taskbarBounds.Top,
            Math.Max(taskbarBounds.Top, taskbarBounds.Bottom - height));

        return new Drawing.Rectangle(left, top, width, height);
    }
}
