using System.Runtime.InteropServices;
using LifeCutdown.App.Models;
using Drawing = System.Drawing;
using Drawing2D = System.Drawing.Drawing2D;
using Forms = System.Windows.Forms;

namespace LifeCutdown.App.Services;

public static class TrayIconRenderer
{
    public static Drawing.Icon CreateProgressIcon(MetricSnapshot metric, TrayIconMetricMode mode)
    {
        var iconWidth = Math.Max(16, Forms.SystemInformation.SmallIconSize.Width);
        var iconHeight = Math.Max(16, Forms.SystemInformation.SmallIconSize.Height);

        using var bitmap = new Drawing.Bitmap(iconWidth, iconHeight, Drawing.Imaging.PixelFormat.Format32bppArgb);
        using var graphics = Drawing.Graphics.FromImage(bitmap);

        graphics.Clear(Drawing.Color.Transparent);
        graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality;
        graphics.CompositingQuality = Drawing2D.CompositingQuality.HighQuality;

        DrawRingIcon(graphics, iconWidth, iconHeight, metric.Percentage, mode);

        return ConvertBitmapToIcon(bitmap);
    }

    public static string BuildTrayText(MetricSnapshot metric)
    {
        return MetricTextFormatter.BuildTrayText(metric);
    }

    private static void DrawRingIcon(Drawing.Graphics graphics, int iconWidth, int iconHeight, double percentage, TrayIconMetricMode mode)
    {
        var size = Math.Min(iconWidth, iconHeight);
        var ringThickness = Math.Max(2.4f, size * 0.18f);
        var padding = Math.Max(2.2f, size * 0.14f);
        var shadowPadding = Math.Max(1f, size * 0.03f);
        var diameter = size - padding * 2;

        var ringBounds = new Drawing.RectangleF(
            padding,
            padding,
            diameter,
            diameter);

        var innerDiameter = Math.Max(2f, diameter - ringThickness * 2 - shadowPadding);
        var innerBounds = new Drawing.RectangleF(
            (iconWidth - innerDiameter) / 2f,
            (iconHeight - innerDiameter) / 2f,
            innerDiameter,
            innerDiameter);

        using var trackPen = new Drawing.Pen(Drawing.Color.FromArgb(255, 226, 221, 212), ringThickness)
        {
            StartCap = Drawing2D.LineCap.Round,
            EndCap = Drawing2D.LineCap.Round,
            Alignment = Drawing2D.PenAlignment.Center,
        };

        using var progressPen = new Drawing.Pen(GetModeColor(mode), ringThickness)
        {
            StartCap = Drawing2D.LineCap.Round,
            EndCap = Drawing2D.LineCap.Round,
            Alignment = Drawing2D.PenAlignment.Center,
        };

        using var centerShadowBrush = new Drawing.SolidBrush(Drawing.Color.FromArgb(34, 0, 0, 0));
        using var centerBrush = new Drawing.SolidBrush(Drawing.Color.FromArgb(248, 247, 243, 236));
        using var borderPen = new Drawing.Pen(Drawing.Color.FromArgb(45, 32, 32, 36), Math.Max(1f, size * 0.03f));

        var shadowBounds = new Drawing.RectangleF(
            innerBounds.X,
            innerBounds.Y + shadowPadding,
            innerBounds.Width,
            innerBounds.Height);

        graphics.DrawArc(trackPen, ringBounds, -90f, 360f);

        var sweepAngle = (float)(Math.Clamp(percentage, 0, 100) / 100.0 * 360.0);
        if (sweepAngle > 0.2f)
        {
            graphics.DrawArc(progressPen, ringBounds, -90f, sweepAngle);
        }

        graphics.FillEllipse(centerShadowBrush, shadowBounds);
        graphics.FillEllipse(centerBrush, innerBounds);
        graphics.DrawEllipse(borderPen, innerBounds);

        DrawProgressMarker(graphics, ringBounds, percentage, mode, ringThickness);
    }

    private static void DrawProgressMarker(Drawing.Graphics graphics, Drawing.RectangleF ringBounds, double percentage, TrayIconMetricMode mode, float ringThickness)
    {
        var clampedPercentage = Math.Clamp(percentage, 0, 100);
        if (clampedPercentage <= 0 || clampedPercentage >= 100)
        {
            return;
        }

        var angle = -90 + clampedPercentage / 100.0 * 360.0;
        var radius = ringBounds.Width / 2f;
        var centerX = ringBounds.X + radius;
        var centerY = ringBounds.Y + radius;
        var radians = Math.PI * angle / 180.0;
        var markerCenterX = centerX + (float)(Math.Cos(radians) * radius);
        var markerCenterY = centerY + (float)(Math.Sin(radians) * radius);
        var markerSize = Math.Max(2.2f, ringThickness * 0.66f);

        var markerBounds = new Drawing.RectangleF(
            markerCenterX - markerSize / 2f,
            markerCenterY - markerSize / 2f,
            markerSize,
            markerSize);

        using var markerBrush = new Drawing.SolidBrush(GetModeColor(mode));
        using var markerBorderPen = new Drawing.Pen(Drawing.Color.FromArgb(245, 247, 243, 236), Math.Max(0.8f, ringThickness * 0.16f));

        graphics.FillEllipse(markerBrush, markerBounds);
        graphics.DrawEllipse(markerBorderPen, markerBounds);
    }

    private static Drawing.Color GetModeColor(TrayIconMetricMode mode)
    {
        return mode switch
        {
            TrayIconMetricMode.Life => Drawing.Color.FromArgb(255, 72, 90, 140),
            TrayIconMetricMode.Year => Drawing.Color.FromArgb(255, 68, 112, 84),
            TrayIconMetricMode.Month => Drawing.Color.FromArgb(255, 123, 88, 52),
            TrayIconMetricMode.Week => Drawing.Color.FromArgb(255, 32, 32, 36),
            TrayIconMetricMode.Day => Drawing.Color.FromArgb(255, 110, 60, 128),
            TrayIconMetricMode.CustomCountdown => Drawing.Color.FromArgb(255, 149, 68, 68),
            _ => Drawing.Color.FromArgb(255, 32, 32, 36),
        };
    }

    private static Drawing.Icon ConvertBitmapToIcon(Drawing.Bitmap bitmap)
    {
        var iconHandle = bitmap.GetHicon();

        try
        {
            using var icon = Drawing.Icon.FromHandle(iconHandle);
            return (Drawing.Icon)icon.Clone();
        }
        finally
        {
            DestroyIcon(iconHandle);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr handle);
}
