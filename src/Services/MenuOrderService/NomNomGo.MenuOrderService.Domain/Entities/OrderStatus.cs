using System.ComponentModel.DataAnnotations;

namespace NomNomGo.MenuOrderService.Domain.Entities;

public class OrderStatus : BaseEntity
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty; // Pending, Confirmed, Preparing, Ready, InDelivery, Delivered, Cancelled
}