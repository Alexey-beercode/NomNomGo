using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomNomGo.IdentityService.Application.Interfaces.Services;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories;
using NomNomGo.IdentityService.Domain.Interfaces.Repositories.Base;
using NomNomGo.IdentityService.Domain.Interfaces.UnitOfWork;
using NomNomGo.IdentityService.Infrastructure.Persistence.Database;
using NomNomGo.IdentityService.Infrastructure.Repositories;
using NomNomGo.IdentityService.Infrastructure.Repositories.Base;
using NomNomGo.IdentityService.Infrastructure.Services;

namespace NomNomGo.IdentityService.Infrastructure.Extensions
{
    /// <summary>
    /// Методы расширения для регистрации сервисов инфраструктурного слоя
    /// </summary>
    public static class InfrastructureServiceRegistration
    {
        /// <summary>
        /// Добавляет сервисы инфраструктурного слоя в контейнер внедрения зависимостей
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="configuration">Конфигурация приложения</param>
        /// <returns>Коллекция сервисов</returns>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Регистрация контекста базы данных
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("IdentityServiceConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Регистрация репозиториев
            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
            services.AddScoped<IEmailService,EmailService>();

            // Регистрация Redis
            // Регистрация Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetSection("Redis")["ConnectionString"];
                options.InstanceName = configuration.GetSection("Redis")["InstanceName"];
            });
            

            // Регистрация MassTransit
            services.AddMassTransit(configure =>
            {
    
                // Настройка RabbitMQ
                configure.UsingRabbitMq((context, cfg) =>
                {
                    // Настройка подключения к RabbitMQ
                    cfg.Host(configuration["RabbitMQ:Host"], configuration["RabbitMQ:VirtualHost"], h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"]);
                        h.Password(configuration["RabbitMQ:Password"]);
                    });

                    // Настройка конечных точек
                    cfg.ReceiveEndpoint("nomnom-identity-service", e =>
                    {
                        // Настройка постоянства сообщений
                        e.PrefetchCount = 16;
                        e.UseMessageRetry(r => r.Interval(2, 100));
                        
                    });
        
                    // Автоматическое создание очередей и обменников при запуске
                    cfg.ConfigureEndpoints(context);
                });
            });
            

            // Регистрация кэширования
            services.AddScoped<ICacheService, CacheService>();

            // Регистрация сервисов для токенов
            services.AddTransient<ITokenService, TokenService>();
            
            // Регистрация сервисов для RabbitMQ
            services.AddScoped<IServiceTokenProvider, ServiceTokenProvider>();
            
            // Регистрация HTTP клиентов для межсервисной коммуникации
            services.AddHttpClient();
            services.AddTransient<ServiceAuthenticationHandler>();

            return services;
        }
    }
}