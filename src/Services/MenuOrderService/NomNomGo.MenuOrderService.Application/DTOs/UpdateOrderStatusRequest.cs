namespace NomNomGo.MenuOrderService.Application.DTOs;

public class UpdateOrderStatusRequest
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Confirmed, Preparing, Ready, InDelivery, Delivered, Cancelled
    public string? Comment { get; set; }
}