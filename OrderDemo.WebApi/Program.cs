using OrderDemo.Application;
using OrderDemo.Infrastructure;
using OrderDemo.WebApi.Extensions;
using OrderDemo.Infrastructure.Persistence.Contexts;


namespace OrderDemo.WebApi {
    public class Program {
        public static void Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpsRedirection(options => {
                options.HttpsPort = 7277;
            });

            // Configure Dependency Injection for Application and Infrastructure layers.
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices();

            WebApplication app = builder.Build();

            // Seed initial data to the in-memory database.
            using (IServiceScope scope = app.Services.CreateScope()) {
                OrderDemoContext context = scope.ServiceProvider.GetRequiredService<OrderDemoContext>();
                context.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Redirect root to /swagger.
            app.MapGet("/", context => {
                context.Response.Redirect("/swagger", permanent: true);
                return Task.CompletedTask;
            });

            // Map Minimal API endpoints.
            app.MapEndPoints();

            app.Run();
        }
    }
}