using System.Globalization;
using LifeCutdown.App.Helpers;

namespace LifeCutdown.App.Models;

public sealed class ProgressMetric : ObservableObject
{
    private double _percentage;
    private string _caption = string.Empty;
    private string _detail = string.Empty;

    public ProgressMetric(string title)
    {
        Title = title;
    }

    public string Title { get; }

    public double Percentage
    {
        get => _percentage;
        set
        {
            if (SetProperty(ref _percentage, Math.Clamp(value, 0, 100)))
            {
                OnPropertyChanged(nameof(PercentText));
            }
        }
    }

    public string PercentText => Percentage.ToString("0.0", CultureInfo.InvariantCulture);

    public string Caption
    {
        get => _caption;
        set => SetProperty(ref _caption, value);
    }

    public string Detail
    {
        get => _detail;
        set => SetProperty(ref _detail, value);
    }
}
