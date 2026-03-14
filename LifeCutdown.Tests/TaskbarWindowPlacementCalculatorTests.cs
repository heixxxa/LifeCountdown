using Drawing = System.Drawing;
using LifeCutdown.App.Services;

namespace LifeCutdown.Tests;

public class TaskbarWindowPlacementCalculatorTests
{
    [Fact]
    public void Calculate_PlacesWindowImmediatelyLeftOfTrayArea()
    {
        var taskbarBounds = new Drawing.Rectangle(0, 1032, 1920, 48);
        var trayBounds = new Drawing.Rectangle(1760, 1032, 160, 48);
        var windowSize = new Drawing.Size(128, 28);

        var placement = TaskbarWindowPlacementCalculator.Calculate(taskbarBounds, trayBounds, windowSize);

        Assert.Equal(1760 - 128 - 6, placement.Left);
        Assert.Equal(1032 + 10, placement.Top);
        Assert.Equal(windowSize.Width, placement.Width);
        Assert.Equal(windowSize.Height, placement.Height);
    }

    [Fact]
    public void Calculate_UsesFallbackPaddingWhenTrayBoundsAreMissing()
    {
        var taskbarBounds = new Drawing.Rectangle(0, 1032, 1920, 48);
        var windowSize = new Drawing.Size(120, 28);

        var placement = TaskbarWindowPlacementCalculator.Calculate(taskbarBounds, null, windowSize);

        Assert.Equal(1920 - 88 - 120 - 6, placement.Left);
    }

    [Fact]
    public void Calculate_ClampsWithinTaskbarBoundsWhenAnchorIsTooFarLeft()
    {
        var taskbarBounds = new Drawing.Rectangle(0, 1032, 1920, 48);
        var trayBounds = new Drawing.Rectangle(60, 1032, 40, 48);
        var windowSize = new Drawing.Size(200, 40);

        var placement = TaskbarWindowPlacementCalculator.Calculate(taskbarBounds, trayBounds, windowSize);

        Assert.Equal(6, placement.Left);
        Assert.Equal(1036, placement.Top);
    }

    [Fact]
    public void SupportsTaskbarWindow_ReturnsFalseForVerticalTaskbar()
    {
        var taskbarBounds = new Drawing.Rectangle(1880, 0, 40, 1080);

        Assert.False(TaskbarWindowPlacementCalculator.SupportsTaskbarWindow(taskbarBounds));
    }
}
