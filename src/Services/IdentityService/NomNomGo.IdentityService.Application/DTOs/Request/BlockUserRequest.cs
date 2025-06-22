using System.ComponentModel.DataAnnotations;

namespace NomNomGo.IdentityService.Application.DTOs.Request;

public class BlockUserRequest
{
    /// <summary>
    /// Причина блокировки
    /// </summary>
    [MaxLength(500, ErrorMessage = "Причина блокировки не должна превышать 500 символов")]
    public string? Reason { get; set; }

    /// <summary>
    /// Продолжительность блокировки: 1h, 1d, 7d, 30d, permanent
    /// </summary>
    [RegularExpression(@"^(1h|1d|7d|30d|permanent)$", 
        ErrorMessage = "Недопустимая продолжительность. Допустимые значения: 1h, 1d, 7d, 30d, permanent")]
    public string? Duration { get; set; }
}