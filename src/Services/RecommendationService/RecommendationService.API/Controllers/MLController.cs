using Microsoft.AspNetCore.Mvc;
using RecommendationService.Application.DTOs;
using RecommendationService.Application.Services;

namespace RecommendationService.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class MLController : ControllerBase
{
    private readonly IMlService _mlService;

    public MLController(IMlService mlService)
    {
        _mlService = mlService;
    }

    [HttpPost("train")]
    public async Task<IActionResult> TrainModels()
    {
        await _mlService.TrainModelsAsync();
        return Ok("Models trained successfully");
    }

    [HttpPost("analyze-sentiment")]
    public async Task<IActionResult> AnalyzeSentiment([FromBody] AnalyzeSentimentRequest request)
    {
        var result = await _mlService.AnalyzeSentimentAsync(request.Text);
        return Ok(new { 
            IsPositive = result.Prediction, 
            Confidence = result.Probability,
            Score = result.Score 
        });
    }
}