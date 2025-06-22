namespace NomNomGo.MenuOrderService.Application.DTOs;

public class RatingResponse
{
    public Guid TargetId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime LastUpdated { get; set; }
}