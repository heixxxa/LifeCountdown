using System.Globalization;
using LifeCountdown.App.Helpers;

namespace LifeCountdown.App.Models;

public sealed class ProgressMetric : ObservableObject
{
    private string _title;
    private double _percentage;
    private string _caption = string.Empty;
    private string _detail = string.Empty;

    public ProgressMetric(string title)
    {
        _title = title;
    }

    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                OnPropertyChanged(nameof(TooltipText));
                OnPropertyChanged(nameof(PercentLabel));
            }
        }
    }

    public double Percentage
    {
        get => _percentage;
        set
        {
            if (SetProperty(ref _percentage, Math.Clamp(value, 0, 100)))
            {
                OnPropertyChanged(nameof(PercentText));
                OnPropertyChanged(nameof(PercentLabel));
            }
        }
    }

    public string PercentText => Percentage.ToString("0.0", CultureInfo.InvariantCulture);

    public string PercentLabel => $"{PercentText}%";

    public string Caption
    {
        get => _caption;
        set
        {
            if (SetProperty(ref _caption, value))
            {
                OnPropertyChanged(nameof(TooltipText));
            }
        }
    }

    public string Detail
    {
        get => _detail;
        set
        {
            if (SetProperty(ref _detail, value))
            {
                OnPropertyChanged(nameof(TooltipText));
            }
        }
    }

    public string TooltipText
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Caption))
            {
                return Detail;
            }

            if (string.IsNullOrWhiteSpace(Detail))
            {
                return Caption;
            }

            return $"{Caption}\n{Detail}";
        }
    }
}
