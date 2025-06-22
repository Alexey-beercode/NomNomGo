using Microsoft.AspNetCore.Mvc;
using RecommendationService.Application.DTOs;
using RecommendationService.Application.Services;

namespace RecommendationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<ActionResult<ReviewResponse>> CreateReview([FromBody] CreateReviewRequest request)
    {
        try
        {
            var review = await _reviewService.CreateReviewAsync(request);
            return Ok(review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{targetId}/{targetType}")]
    public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetReviews(Guid targetId, string targetType)
    {
        var reviews = await _reviewService.GetReviewsAsync(targetId, targetType);
        return Ok(reviews);
    }

    [HttpGet("rating/{targetId}/{targetType}")]
    public async Task<ActionResult<RatingResponse>> GetRating(Guid targetId, string targetType)
    {
        var rating = await _reviewService.GetRatingAsync(targetId, targetType);
        return Ok(rating);
    }

    [HttpPost("update-ratings")]
    public async Task<IActionResult> UpdateRatings()
    {
        await _reviewService.UpdateRatingsAsync();
        return Ok("Ratings updated successfully");
    }
}