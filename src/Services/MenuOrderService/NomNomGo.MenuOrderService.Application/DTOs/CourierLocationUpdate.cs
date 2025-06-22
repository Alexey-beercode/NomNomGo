namespace NomNomGo.MenuOrderService.Application.DTOs;

public class CourierLocationUpdate
{
    public Guid CourierId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}