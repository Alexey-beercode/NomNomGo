using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NomNomGo.IdentityService.API.Middleware;
using Serilog;

namespace NomNomGo.IdentityService.API.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Configure application pipeline
    /// </summary>
    /// <summary>
    /// Configure application pipeline
    /// </summary>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Логирование запросов (должно быть в самом начале)
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        // Обработка ошибок (должна быть рано в пайплайне)
        app.UseMiddleware<ErrorHandlingMiddleware>();

        // Настройка для разработки
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "NomNomGo Identity Service API v1");
                c.RoutePrefix = "swagger"; // Swagger доступен по /swagger
            });
        }

        // CORS ДОЛЖЕН БЫТЬ ПЕРЕД UseRouting()!
        // Используем самую открытую политику для разработки
        app.UseCors("AllowAll");

        // Маршрутизация
        app.UseRouting();

        // Аутентификация и авторизация (порядок важен!)
        app.UseAuthentication();
        app.UseAuthorization();

        // Health checks
        app.UseHealthChecks();

        // Контроллеры
        app.MapControllers();

        return app;
    }

    public static WebApplication UseHealthChecks(this WebApplication app)
    {
        // Основные проверки
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    
        // Специальные эндпоинты для отдельных компонентов
        app.MapHealthChecks("/health/db", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("db"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    
        app.MapHealthChecks("/health/cache", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("cache"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    
        app.MapHealthChecks("/health/messaging", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("messaging"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        // Общий health check endpoint
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    
        return app;
    }
}