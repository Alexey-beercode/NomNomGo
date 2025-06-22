using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NomNomGo.IdentityService.API.Services;
using NomNomGo.IdentityService.Application.Extensions;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Infrastructure.Extensions;
using NomNomGo.IdentityService.Infrastructure.HealthChecks;
using Serilog;
using Serilog.Events;

namespace NomNomGo.IdentityService.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Configures services for the application
    /// </summary>
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        // Регистрация HTTP контекста
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices(builder.Configuration);
        
        builder.ConfigureAuthentication(builder.Configuration);
        builder.ConfigureAuthorization(); // Новый метод для настройки авторизации

        builder.ConfigureCors();
        builder.ConfigureDependencies();
        
        builder.ConfigureHealthChecks();
        builder.ConfigureSerilog();
        builder.ConfigureSwagger();

        return builder;
    }

    public static WebApplicationBuilder ConfigureDependencies(this WebApplicationBuilder builder)
    {
        // Регистрация сервиса текущего пользователя
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        return builder;
    }

    /// <summary>
    /// Настройка авторизации и политик доступа
    /// </summary>
    public static WebApplicationBuilder ConfigureAuthorization(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            // Политика для сервисных запросов (межсервисная коммуникация)
            options.AddPolicy("ServicePolicy", policy =>
            {
                policy.RequireClaim("client_id");
                policy.RequireRole("service");
            });
            
            // Политика только для администраторов
            options.AddPolicy("AdminPolicy", policy =>
            {
                policy.RequireRole("Admin");
            });
            
            // Политика для администраторов и модераторов
            options.AddPolicy("ModeratorPolicy", policy =>
            {
                policy.RequireRole("Admin", "Moderator");
            });
            
            // Политика для пользователей (базовая аутентификация)
            options.AddPolicy("UserPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
            
            // Политика для проверки разрешений
            options.AddPolicy("ManageUsersPermission", policy =>
            {
                policy.RequireClaim("permission", "manage_users");
            });
            
            options.AddPolicy("ViewUsersPermission", policy =>
            {
                policy.RequireClaim("permission", "view_users");
            });
        });

        return builder;
    }

    /// <summary>
    /// Configures additional health checks
    /// </summary>
    public static WebApplicationBuilder ConfigureHealthChecks(this WebApplicationBuilder builder)
    {
        var healthConfig = builder.Configuration.GetSection("HealthChecks");
        var timeoutSeconds = healthConfig.GetValue<int>("TimeoutSeconds", 10);
    
        var services = builder.Services;
    
        // Параметры из конфигурации
        var postgresConnection = builder.Configuration.GetConnectionString("IdentityServiceConnection");
        var redisConnection = builder.Configuration.GetSection("Redis")["ConnectionString"];
        var rabbitMQConfig = builder.Configuration.GetSection("RabbitMQ");
    
        // Создание экземпляров Health Check классов
        services.AddSingleton<IHealthCheck>(new PostgresHealthCheck(
            postgresConnection,
            timeoutSeconds));
    
        services.AddSingleton<IHealthCheck>(new RedisHealthCheck(
            redisConnection));
    
        services.AddSingleton<IHealthCheck>(new RabbitMQHealthCheck(
            rabbitMQConfig["Host"],
            rabbitMQConfig["VirtualHost"],
            rabbitMQConfig["Username"],
            rabbitMQConfig["Password"]));

        // Register health checks with configuration settings 
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
            .AddCheck<PostgresHealthCheck>("postgres-db", tags: new[] { "ready", "db" })
            .AddCheck<RedisHealthCheck>("redis-cache", tags: new[] { "ready", "cache" })
            .AddCheck<RabbitMQHealthCheck>("rabbitmq", tags: new[] { "ready", "messaging" });

        return builder;
    }

    /// <summary>
    /// Configure serilog for logging
    /// </summary>
    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .ConfigureDefaultLogging(builder.Environment)
            .CreateLogger();
        
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);
        builder.Host.UseSerilog();

        return builder;
    }

    public static WebApplicationBuilder ConfigureAuthentication(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        // Настройка аутентификации
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"];
            
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // Убираем погрешность времени
                };
                
                // Настройка событий для более детального логирования
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Log.Debug("JWT Token validated for user: {User}", 
                            context.Principal?.Identity?.Name ?? "Unknown");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Log.Warning("JWT Authentication challenge: {Scheme} {Parameter}", 
                            context.Scheme.Name, context.AuthenticateFailure?.Message);
                        return Task.CompletedTask;
                    }
                };
            });

        return builder;
    }

    public static WebApplicationBuilder ConfigureSwagger(this WebApplicationBuilder builder)
    {
        // Настройка Swagger
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "NomNomGo Identity Service API", 
                Version = "v1",
                Description = "API для управления пользователями и аутентификацией в системе NomNomGo",
                Contact = new OpenApiContact
                {
                    Name = "NomNomGo Development Team",
                    Email = "dev@nomnom.go"
                }
            });
                
            // Настройка авторизации в Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
                
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            
            // Включение XML комментариев (если нужно)
            // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            // c.IncludeXmlComments(xmlPath);
        });

        return builder;
    }
    
   /// <summary>
/// Настройка CORS для разрешения запросов с фронтенда
/// </summary>
/// <summary>
/// Настройка CORS для разрешения запросов с фронтенда
/// </summary>
public static WebApplicationBuilder ConfigureCors(this WebApplicationBuilder builder)
{
    builder.Services.AddCors(options =>
    {
        // ПОЛНОСТЬЮ ОТКРЫТАЯ политика для разработки
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()      // Разрешить ВСЕ домены
                .AllowAnyMethod()        // Разрешить ВСЕ методы (GET, POST, PUT, DELETE, OPTIONS)
                .AllowAnyHeader();       // Разрешить ВСЕ заголовки
        });

        // Политика для разработки с credentials (если нужны cookies)
        options.AddPolicy("Development", policy =>
        {
            policy.SetIsOriginAllowed(origin => true)  // Разрешить ВСЕ origins
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });

        // Политика для продакшена (когда будете готовы)
        options.AddPolicy("Production", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200",
                    "https://localhost:4200",
                    "http://127.0.0.1:4200",
                    "https://127.0.0.1:4200",
                    "capacitor://localhost",
                    "ionic://localhost",
                    "http://localhost",
                    "https://localhost"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    return builder;
}
}