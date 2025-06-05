using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderDemo.Application.Interfaces.Contexts; 
using OrderDemo.Infrastructure.Persistence.Contexts; 

namespace OrderDemo.Infrastructure {
    /// <summary>
    /// Provides extension methods for configuring infrastructure layer services.
    /// </summary>
    public static class DependencyInjection {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services) {
            // Configure DbContext with In-Memory Database
            services.AddDbContext<IOrderDemoContext, OrderDemoContext>(options => {
                options.UseInMemoryDatabase("OrderDemoDb"); 
            });

            return services;
        }
    }
}