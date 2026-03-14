using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LifeCutdown.App.Services;

public sealed class SystemTrayController
{
    private const int SwHide = 0;
    private const int SwShow = 5;

    public bool ToggleOverflowWindow()
    {
        var overflowWindow = FindWindow("NotifyIconOverflowWindow", null);
        if (overflowWindow == IntPtr.Zero)
        {
            return false;
        }

        if (IsWindowVisible(overflowWindow))
        {
            return ShowWindow(overflowWindow, SwHide);
        }

        var shown = ShowWindow(overflowWindow, SwShow);
        SetForegroundWindow(overflowWindow);
        return shown || IsWindowVisible(overflowWindow);
    }

    public bool TryOpenTraySettings()
    {
        return TryOpenUri("ms-settings:taskbar") || TryOpenUri("ms-settings:personalization-taskbar");
    }

    private static bool TryOpenUri(string uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri)
            {
                UseShellExecute = true,
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr FindWindow(string? className, string? windowName);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int command);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}
