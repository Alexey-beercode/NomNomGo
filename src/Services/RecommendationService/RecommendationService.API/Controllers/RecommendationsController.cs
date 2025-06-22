using Microsoft.AspNetCore.Mvc;
using RecommendationService.Application.DTOs;
using RecommendationService.Application.Services;

namespace RecommendationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet("personalized/{userId}")]
    public async Task<ActionResult<IEnumerable<RecommendationResponse>>> GetPersonalizedRecommendations(
        Guid userId, [FromQuery] int count = 10)
    {
        var recommendations = await _recommendationService.GetPersonalizedRecommendationsAsync(userId, count);
        return Ok(recommendations);
    }

    [HttpGet("popular")]
    public async Task<ActionResult<IEnumerable<RecommendationResponse>>> GetPopularRecommendations(
        [FromQuery] int count = 10)
    {
        var recommendations = await _recommendationService.GetPopularRecommendationsAsync(count);
        return Ok(recommendations);
    }

    [HttpPost("order")]
    public async Task<IActionResult> AddUserOrder([FromBody] AddOrderRequest request)
    {
        await _recommendationService.AddUserOrderAsync(request.UserId, request.RestaurantId, request.MenuItemId);
        return Ok("Order added to history");
    }

    [HttpPost("update-popular")]
    public async Task<IActionResult> UpdatePopularItems()
    {
        await _recommendationService.UpdatePopularItemsAsync();
        return Ok("Popular items updated");
    }
}