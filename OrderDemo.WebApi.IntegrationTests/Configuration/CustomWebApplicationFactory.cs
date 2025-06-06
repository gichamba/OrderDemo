using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderDemo.Infrastructure.Persistence.Contexts;
using OrderDemo.Domain.Entities;
using OrderDemo.Domain.Enums;
using System.Reflection;

namespace OrderDemo.WebApi.IntegrationTests.Configuration {
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class {
        // Ensure a unique database name for each factory instance (which usually means each test class).
        // This ensures true isolation between different runs of the factory.
        private static readonly string _inMemoryDbName = "OrderDemoTestDatabase-" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            builder.UseEnvironment("Development");

            // Get the path to the currently executing assembly (the integration test assembly).
            string? integrationTestsAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Navigate up from the test assembly's bin directory to the solution root.
            string solutionRoot = Path.Combine(integrationTestsAssemblyPath!, "..", "..", "..", "..");

            // Now, construct the path to the 'OrderDemo.WebApi' project folder relative to the solution root.
            string webApiContentRoot = Path.Combine(solutionRoot, "OrderDemo.WebApi");

            builder.UseContentRoot(webApiContentRoot);

            builder.ConfigureServices(services => {
                ServiceDescriptor? descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<OrderDemoContext>));

                if (descriptor != null) {
                    services.Remove(descriptor);
                }

                services.AddDbContext<OrderDemoContext>(options => {
                    options.UseInMemoryDatabase(_inMemoryDbName);
                });

                ServiceProvider sp = services.BuildServiceProvider();

                using IServiceScope scope = sp.CreateScope();
                IServiceProvider scopedServices = scope.ServiceProvider;
                OrderDemoContext db = scopedServices.GetRequiredService<OrderDemoContext>();
                ILogger logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

                db.Database.EnsureCreated();

                // Clear existing data to ensure a clean state for each test run.
                db.Orders.RemoveRange(db.Orders);
                db.Customers.RemoveRange(db.Customers);
                db.SaveChanges();

                // Seed data.
                List<Customer> customersToSeed =
                [
                    new() { Id = 1, Name = "Alice Smith", CustomerSegment = CustomerSegment.New },
                    new() { Id = 2, Name = "Bob Johnson", CustomerSegment = CustomerSegment.Loyal },
                    new() { Id = 3, Name = "Charlie Brown", CustomerSegment = CustomerSegment.Regular },
                    new() { Id = 4, Name = "Diana Prince", CustomerSegment = CustomerSegment.Loyal },
                    new() { Id = 5, Name = "Eve Green", CustomerSegment = CustomerSegment.New }
                ];
                db.Customers.AddRange(customersToSeed);
                db.SaveChanges();

                // Manually create some orders for loyal customers if needed for their discount logic.
                Customer? bob = db.Customers.FirstOrDefault(c => c.Id == 2);
                if (bob != null) {
                    for (int i = 0; i < 10; i++) {
                        db.Orders.Add(new Order {
                            CustomerId = bob.Id,
                            TotalAmount = 50.00m,
                            OrderDate = DateTime.UtcNow.AddDays(-(i + 1)),
                            OrderStatus = OrderStatus.Delivered,
                            DiscountAmount = 0.00m
                        });
                    }
                    db.SaveChanges();
                }

                // DEBUGGING STEP: Verify seeded customers exist immediately after creation.
                try {
                    List<Customer> customers = db.Customers.Include(c => c.Orders).ToList();
                    logger.LogInformation("DEBUG_FACTORY: Customers seeded in DbContext (after EnsureCreated):");
                    if (!customers.Any()) {
                        logger.LogWarning("DEBUG_FACTORY: No customers found after initial seeding!");
                    }
                    foreach (Customer customer in customers) {
                        logger.LogInformation("DEBUG_FACTORY: Customer ID: {Id}, Name: {Name}, Segment: {Segment}, Orders: {OrderCount}",
                            customer.Id, customer.Name, customer.CustomerSegment, customer.Orders?.Count ?? 0);
                    }

                    if (db.Customers.Any(c => c.Id == 1)) {
                        logger.LogInformation("DEBUG_FACTORY: Customer with ID 1 found.");
                    } else {
                        logger.LogError("DEBUG_FACTORY: Customer with ID 1 NOT found!");
                    }

                    if (db.Customers.Any(c => c.Id == 5)) {
                        logger.LogInformation("DEBUG_FACTORY: Customer with ID 5 found.");
                    } else {
                        logger.LogError("DEBUG_FACTORY: Customer with ID 5 NOT found!");
                    }
                } catch (Exception ex) {
                    logger.LogError(ex, "DEBUG_FACTORY: Error during factory customer verification after EnsureCreated.");
                }
            });
        }
    }
}