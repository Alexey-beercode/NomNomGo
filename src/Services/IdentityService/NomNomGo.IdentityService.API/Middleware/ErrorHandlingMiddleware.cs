using System.Net;
using System.Text.Json;
using NomNomGo.IdentityService.Domain.Exceptions;

namespace NomNomGo.IdentityService.API.Middleware
{
    /// <summary>
    /// Middleware для глобальной обработки исключений
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Произошло необработанное исключение: {Message}", exception.Message);

            var response = context.Response;
            response.ContentType = "application/json";

            var (statusCode, error) = exception switch
            {
                ValidationException validationEx => (HttpStatusCode.BadRequest, new
                {
                    Title = "Ошибка валидации",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = "Произошли одна или несколько ошибок валидации.",
                    Errors = validationEx.Errors
                }),
                
                NotFoundException notFoundEx => (HttpStatusCode.NotFound, new
                {
                    Title = "Ресурс не найден",
                    Status = (int)HttpStatusCode.NotFound,
                    Detail = notFoundEx.Message
                }),
                
                AuthenticationException authEx => (HttpStatusCode.Unauthorized, new
                {
                    Title = "Ошибка аутентификации",
                    Status = (int)HttpStatusCode.Unauthorized,
                    Detail = authEx.Message
                }),
                
                UnauthorizedAccessException => (HttpStatusCode.Forbidden, new
                {
                    Title = "Доступ запрещен",
                    Status = (int)HttpStatusCode.Forbidden,
                    Detail = "У вас нет прав для выполнения этой операции."
                }),
                
                _ => _environment.IsDevelopment()
                    ? ((object statusCode, object error))(HttpStatusCode.InternalServerError, new
                    {
                        Title = "Внутренняя ошибка сервера",
                        Status = (int)HttpStatusCode.InternalServerError,
                        Detail = exception.Message,
                        StackTrace = exception.StackTrace
                    })
                    : (HttpStatusCode.InternalServerError, new
                    {
                        Title = "Внутренняя ошибка сервера",
                        Status = (int)HttpStatusCode.InternalServerError,
                        Detail = "Произошла внутренняя ошибка сервера. Пожалуйста, повторите попытку позже."
                    })
            };

            response.StatusCode = (int)statusCode;
            
            var result = JsonSerializer.Serialize(error, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(result);
        }
    }
}