namespace RecommendationService.ML.Models;

public class SentimentData
{
    public string Text { get; set; } = string.Empty;
    public bool Sentiment { get; set; } // true = positive, false = negative
}