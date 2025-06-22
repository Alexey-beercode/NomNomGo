using Microsoft.AspNetCore.Mvc;
using NomNomGo.MenuOrderService.Application.DTOs;
using NomNomGo.MenuOrderService.Application.Services;

namespace NomNomGo.MenuOrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantsController(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RestaurantResponse>>> GetRestaurants()
    {
        var restaurants = await _restaurantService.GetAllRestaurantsAsync();
        return Ok(restaurants);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RestaurantResponse>> GetRestaurant(Guid id)
    {
        var restaurant = await _restaurantService.GetRestaurantByIdAsync(id);
        return restaurant == null ? NotFound() : Ok(restaurant);
    }

    [HttpPost]
    public async Task<ActionResult<RestaurantResponse>> CreateRestaurant([FromBody] CreateRestaurantRequest request)
    {
        var restaurant = await _restaurantService.CreateRestaurantAsync(request);
        return CreatedAtAction(nameof(GetRestaurant), new { id = restaurant.Id }, restaurant);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRestaurant(Guid id, [FromBody] CreateRestaurantRequest request)
    {
        var updated = await _restaurantService.UpdateRestaurantAsync(id, request);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRestaurant(Guid id)
    {
        var deleted = await _restaurantService.DeleteRestaurantAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}