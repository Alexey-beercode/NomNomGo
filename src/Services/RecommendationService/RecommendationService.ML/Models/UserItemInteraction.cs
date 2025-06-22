namespace RecommendationService.ML.Models;

public class UserItemInteraction
{
    public float UserId { get; set; }
    public float ItemId { get; set; }
    public float Rating { get; set; } // Конвертируем из количества заказов
}