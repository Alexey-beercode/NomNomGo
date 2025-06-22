using Microsoft.AspNetCore.Mvc;
using NomNomGo.MenuOrderService.Application.DTOs;
using NomNomGo.MenuOrderService.Application.Services;

namespace NomNomGo.MenuOrderService.API.Controllers;

[ApiController]
[Route("api/menu-items")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<ActionResult<IEnumerable<MenuItemResponse>>> GetRestaurantMenu(Guid restaurantId)
    {
        var menu = await _menuService.GetMenuByRestaurantAsync(restaurantId);
        return Ok(menu);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories()
    {
        var categories = await _menuService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("items/{id}")]
    public async Task<ActionResult<MenuItemResponse>> GetMenuItem(Guid id)
    {
        var item = await _menuService.GetMenuItemByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost("items")]
    public async Task<ActionResult<MenuItemResponse>> CreateMenuItem([FromBody] CreateMenuItemRequest request)
    {
        var item = await _menuService.CreateMenuItemAsync(request);
        return CreatedAtAction(nameof(GetMenuItem), new { id = item.Id }, item);
    }

    [HttpPut("items/{id}")]
    public async Task<IActionResult> UpdateMenuItem(Guid id, [FromBody] CreateMenuItemRequest request)
    {
        var updated = await _menuService.UpdateMenuItemAsync(id, request);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("items/{id}")]
    public async Task<IActionResult> DeleteMenuItem(Guid id)
    {
        var deleted = await _menuService.DeleteMenuItemAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}