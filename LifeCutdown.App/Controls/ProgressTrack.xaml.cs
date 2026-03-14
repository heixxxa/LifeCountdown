using System.Windows;

namespace LifeCutdown.App.Controls;

public partial class ProgressTrack : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty PercentageProperty = DependencyProperty.Register(
        nameof(Percentage),
        typeof(double),
        typeof(ProgressTrack),
        new PropertyMetadata(0d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MarkerHeightProperty = DependencyProperty.Register(
        nameof(MarkerHeight),
        typeof(double),
        typeof(ProgressTrack),
        new PropertyMetadata(20d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MarkerTopOffsetProperty = DependencyProperty.Register(
        nameof(MarkerTopOffset),
        typeof(double),
        typeof(ProgressTrack),
        new PropertyMetadata(-3d));

    public static readonly DependencyProperty TrackBrushProperty = DependencyProperty.Register(
        nameof(TrackBrush),
        typeof(System.Windows.Media.Brush),
        typeof(ProgressTrack),
        new PropertyMetadata(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 227, 216))));

    public static readonly DependencyProperty FillBrushProperty = DependencyProperty.Register(
        nameof(FillBrush),
        typeof(System.Windows.Media.Brush),
        typeof(ProgressTrack),
        new PropertyMetadata(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 32, 36))));

    public static readonly DependencyProperty MarkerBrushProperty = DependencyProperty.Register(
        nameof(MarkerBrush),
        typeof(System.Windows.Media.Brush),
        typeof(ProgressTrack),
        new PropertyMetadata(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 32, 36))));

    public ProgressTrack()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdateVisuals();
        SizeChanged += (_, _) => UpdateVisuals();
    }

    public double Percentage
    {
        get => (double)GetValue(PercentageProperty);
        set => SetValue(PercentageProperty, value);
    }

    public double MarkerHeight
    {
        get => (double)GetValue(MarkerHeightProperty);
        set => SetValue(MarkerHeightProperty, value);
    }

    public double MarkerTopOffset
    {
        get => (double)GetValue(MarkerTopOffsetProperty);
        set => SetValue(MarkerTopOffsetProperty, value);
    }

    public System.Windows.Media.Brush TrackBrush
    {
        get => (System.Windows.Media.Brush)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public System.Windows.Media.Brush FillBrush
    {
        get => (System.Windows.Media.Brush)GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    public System.Windows.Media.Brush MarkerBrush
    {
        get => (System.Windows.Media.Brush)GetValue(MarkerBrushProperty);
        set => SetValue(MarkerBrushProperty, value);
    }

    private static void OnLayoutPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        ((ProgressTrack)dependencyObject).UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (!IsLoaded)
        {
            return;
        }

        var width = TrackRoot.ActualWidth;
        var markerWidth = Marker.Width;
        var percentage = Math.Clamp(Percentage, 0, 100);
        var fillWidth = width * percentage / 100.0;

        FillBar.Width = fillWidth;
        System.Windows.Controls.Canvas.SetLeft(Marker, Math.Clamp(fillWidth - markerWidth / 2.0, 0, Math.Max(0, width - markerWidth)));
    }
}
