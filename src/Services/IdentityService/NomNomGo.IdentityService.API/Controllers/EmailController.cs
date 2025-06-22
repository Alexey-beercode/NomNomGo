using Microsoft.AspNetCore.Mvc;
using NomNomGo.IdentityService.Application.DTOs.Request;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Domain.Models;

namespace NomNomGo.IdentityService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-registration")]
    public async Task<IActionResult> SendRegistrationEmail([FromBody] RegistrationEmailRequest request)
    {
        try
        {
            var emailData = new RegistrationEmail
            {
                ToEmail = request.Email,
                Name = request.Username,
                AppName = "NomNomGo"
            };

            await _emailService.SendRegistrationEmailAsync(emailData);
            return Ok(new { success = true, message = "Email sent successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("send-order-notification")]
    public async Task<IActionResult> SendOrderNotification([FromBody] OrderEmailRequest request)
    {
        try
        {
            var emailData = new OrderEmail
            {
                ToEmail = request.Email,
                Name = request.CustomerName,
                OrderId = request.OrderId,
                RestaurantName = request.RestaurantName,
                TotalPrice = request.TotalPrice,
                Status = request.Status
            };

            await _emailService.SendOrderNotificationAsync(emailData);
            return Ok(new { success = true, message = "Order notification sent" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}