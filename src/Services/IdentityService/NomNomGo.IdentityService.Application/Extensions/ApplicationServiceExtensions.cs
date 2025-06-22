using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NomNomGo.IdentityService.Application.Behaviors;

namespace NomNomGo.IdentityService.Application.Extensions
{
    /// <summary>
    /// Extension methods for registering application services
    /// </summary>
    public static class ApplicationServiceExtensions
    {
        /// <summary>
        /// Adds application services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The service collection with application services added</returns>
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register MediatR
            services.AddMediatR(cfg => 
            {
                // Register services from the current assembly
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                
                // Register pipeline behaviors
                // Validation behavior - validates requests using FluentValidation
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                
            });
            
            // Register FluentValidation validators from the current assembly
           // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Register AutoMapper profiles from the current assembly
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            
            return services;
        }
    }
}