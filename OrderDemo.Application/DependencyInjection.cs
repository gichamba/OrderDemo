using Microsoft.Extensions.DependencyInjection;
using OrderDemo.Application.Interfaces.Services;
using OrderDemo.Application.Services;
using System.Reflection;

namespace OrderDemo.Application {
    /// <summary>
    /// Provides extension methods for configuring application layer services.
    /// </summary>
    public static class DependencyInjection {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services) {
            // Register AutoMapper profiles from the current assembly.
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register application services with a Scoped lifetime.
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IDiscountService, DiscountService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();

            return services;
        }
    }
}