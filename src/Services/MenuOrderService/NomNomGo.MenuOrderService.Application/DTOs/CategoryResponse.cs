namespace NomNomGo.MenuOrderService.Application.DTOs;


public class CategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
}