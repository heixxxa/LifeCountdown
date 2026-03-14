namespace LifeCountdown.App.Models;

public sealed class CustomEventSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Title { get; set; } = "自定义事件";

    public DateTime StartDate { get; set; } = DateTime.Today;

    public DateTime TargetDate { get; set; } = DateTime.Today.AddDays(30);

    public CustomEventSettings Clone()
    {
        return new CustomEventSettings
        {
            Id = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString("N") : Id,
            Title = Title,
            StartDate = StartDate,
            TargetDate = TargetDate,
        };
    }
}
