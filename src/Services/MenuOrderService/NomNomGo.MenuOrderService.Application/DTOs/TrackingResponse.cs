namespace NomNomGo.MenuOrderService.Application.DTOs;

public class TrackingResponse
{
    public Guid OrderId { get; set; }
    public Guid CourierId { get; set; }
    public double? CourierLatitude { get; set; }
    public double? CourierLongitude { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
    public string Status { get; set; } = string.Empty;
}