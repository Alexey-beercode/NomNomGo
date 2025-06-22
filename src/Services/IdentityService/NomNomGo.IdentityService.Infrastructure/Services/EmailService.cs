using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Domain.Models;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace NomNomGo.IdentityService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendRegistrationEmailAsync(RegistrationEmail registrationEmail, CancellationToken cancellationToken = default)
    {
        try
        {
            // Проверяем конфигурацию email
            if (!IsEmailConfigurationValid())
            {
                _logger.LogWarning("Email конфигурация не настроена. Пропускаем отправку email для {Email}", registrationEmail.ToEmail);
                return;
            }

            var senderEmail = _configuration["Email:SenderAddress"];
            
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("NomNomGo", senderEmail));
            emailMessage.To.Add(new MailboxAddress("", registrationEmail.ToEmail));
            emailMessage.Subject = "Добро пожаловать в NomNomGo!";

            var bodyBuilder = new BodyBuilder { HtmlBody = RenderRegistrationEmail(registrationEmail) };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(emailMessage, cancellationToken);
            
            _logger.LogInformation("Email регистрации успешно отправлен на {Email}", registrationEmail.ToEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке email регистрации на {Email}", registrationEmail.ToEmail);
            // Не пробрасываем исключение, чтобы не нарушить процесс регистрации
        }
    }

    public async Task SendOrderNotificationAsync(OrderEmail orderEmail, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsEmailConfigurationValid())
            {
                _logger.LogWarning("Email конфигурация не настроена. Пропускаем отправку email для {Email}", orderEmail.ToEmail);
                return;
            }

            var senderEmail = _configuration["Email:SenderAddress"];
            
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("NomNomGo", senderEmail));
            emailMessage.To.Add(new MailboxAddress("", orderEmail.ToEmail));
            emailMessage.Subject = $"Обновление заказа #{orderEmail.OrderId}";

            var bodyBuilder = new BodyBuilder { HtmlBody = RenderOrderEmail(orderEmail) };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(emailMessage, cancellationToken);
            
            _logger.LogInformation("Email уведомление о заказе успешно отправлен на {Email}", orderEmail.ToEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке email уведомления о заказе на {Email}", orderEmail.ToEmail);
        }
    }

    private bool IsEmailConfigurationValid()
    {
        var senderEmail = _configuration["Email:SenderAddress"];
        var smtpServer = _configuration["Email:SmtpServer"];
        var smtpPort = _configuration["Email:SmtpPort"];
        var senderPassword = _configuration["Email:SmtpPassword"];

        if (string.IsNullOrEmpty(senderEmail))
        {
            _logger.LogWarning("Email:SenderAddress не настроен");
            return false;
        }

        if (string.IsNullOrEmpty(smtpServer))
        {
            _logger.LogWarning("Email:SmtpServer не настроен");
            return false;
        }

        if (string.IsNullOrEmpty(smtpPort) || !int.TryParse(smtpPort, out _))
        {
            _logger.LogWarning("Email:SmtpPort не настроен или имеет неверный формат");
            return false;
        }

        if (string.IsNullOrEmpty(senderPassword))
        {
            _logger.LogWarning("Email:SmtpPassword не настроен");
            return false;
        }

        return true;
    }

    private async Task SendEmailAsync(MimeMessage emailMessage, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient();
        
        var smtpServer = _configuration["Email:SmtpServer"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
        var senderEmail = _configuration["Email:SenderAddress"];
        var senderPassword = _configuration["Email:SmtpPassword"];

        _logger.LogDebug("Подключение к SMTP серверу {SmtpServer}:{SmtpPort}", smtpServer, smtpPort);
        
        await client.ConnectAsync(smtpServer, smtpPort, true, cancellationToken);
        await client.AuthenticateAsync(senderEmail, senderPassword, cancellationToken);
        await client.SendAsync(emailMessage, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
        
        _logger.LogDebug("Email успешно отправлен через SMTP сервер");
    }

    private string RenderRegistrationEmail(RegistrationEmail email)
    {
        return $@"
        <!DOCTYPE html>
        <html lang='ru'>
        <head>
            <meta charset='UTF-8'>
            <title>Добро пожаловать в NomNomGo!</title>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; background-color: #f4f4f4; }}
                .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 10px; }}
                .header {{ background: #2E7D32; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                .content {{ padding: 20px; }}
                .footer {{ background: #f8f9fa; padding: 15px; text-align: center; border-radius: 0 0 10px 10px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>🍕 Добро пожаловать в NomNomGo!</h1>
                </div>
                <div class='content'>
                    <h2>Привет, {email.Name}!</h2>
                    <p>Спасибо за регистрацию в NomNomGo - вашем новом помощнике для заказа еды!</p>
                    <p>Теперь вы можете:</p>
                    <ul>
                        <li>🍔 Заказывать еду из любимых ресторанов</li>
                        <li>📱 Отслеживать доставку в реальном времени</li>
                        <li>💰 Получать персональные скидки и акции</li>
                        <li>⭐ Оставлять отзывы и рейтинги</li>
                    </ul>
                    <p>Приятного аппетита!</p>
                </div>
                <div class='footer'>
                    <p>© 2025 NomNomGo. Все права защищены.</p>
                </div>
            </div>
        </body>
        </html>";
    }

    private string RenderOrderEmail(OrderEmail email)
    {
        var statusText = email.Status switch
        {
            "Pending" => "принят в обработку",
            "Preparing" => "готовится",
            "Ready" => "готов к доставке",
            "InDelivery" => "в пути",
            "Delivered" => "доставлен",
            "Cancelled" => "отменен",
            _ => email.Status
        };

        return $@"
        <!DOCTYPE html>
        <html lang='ru'>
        <head>
            <meta charset='UTF-8'>
            <title>Обновление заказа</title>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; background-color: #f4f4f4; }}
                .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 10px; }}
                .header {{ background: #2E7D32; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                .status {{ background: #e8f5e8; padding: 15px; border-radius: 5px; margin: 15px 0; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>📦 Обновление заказа</h1>
                </div>
                <div class='content'>
                    <h2>Привет, {email.Name}!</h2>
                    <p>Статус вашего заказа изменился:</p>
                    <div class='status'>
                        <h3>Заказ #{email.OrderId}</h3>
                        <p><strong>Ресторан:</strong> {email.RestaurantName}</p>
                        <p><strong>Сумма:</strong> ₽{email.TotalPrice:F2}</p>
                        <p><strong>Статус:</strong> {statusText}</p>
                    </div>
                </div>
            </div>
        </body>
        </html>";
    }
}