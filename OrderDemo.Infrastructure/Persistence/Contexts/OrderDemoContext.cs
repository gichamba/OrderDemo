using Microsoft.EntityFrameworkCore;
using OrderDemo.Application.Interfaces.Contexts;
using OrderDemo.Domain.Entities;
using OrderDemo.Domain.Enums;


namespace OrderDemo.Infrastructure.Persistence.Contexts {
    /// <summary>
    /// The concrete implementation of the database context for the Order Management System,
    /// using Entity Framework Core's InMemory provider.
    /// </summary>
    public class OrderDemoContext(DbContextOptions<OrderDemoContext> options) : DbContext(options), IOrderDemoContext {
        /// <summary>
        /// Gets a DbSet for managing Customer entities.
        /// </summary>
        public DbSet<Customer> Customers { get; set; }

        /// <summary>
        /// Gets a DbSet for managing Order entities.
        /// </summary>
        public DbSet<Order> Orders { get; set; }

        /// <summary>
        /// Configures the model and seeds initial data for the in-memory database.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, Name = "Alice Smith", CustomerSegment = CustomerSegment.New },
                new Customer { Id = 2, Name = "Bob Johnson", CustomerSegment = CustomerSegment.Loyal },
                new Customer { Id = 3, Name = "Charlie Brown", CustomerSegment = CustomerSegment.Regular },
                new Customer { Id = 4, Name = "Diana Prince", CustomerSegment = CustomerSegment.Loyal }
            );

            modelBuilder.Entity<Order>().HasData(
                new Order { Id = 101, CustomerId = 1, OrderDate = DateTime.UtcNow.AddDays(-5), TotalAmount = 150.00m, DiscountAmount = 0.00m, OrderStatus = OrderStatus.Pending },
                new Order { Id = 102, CustomerId = 1, OrderDate = DateTime.UtcNow.AddDays(-4), TotalAmount = 25.50m, DiscountAmount = 0.00m, OrderStatus = OrderStatus.Processing },

                new Order { Id = 201, CustomerId = 2, OrderDate = DateTime.UtcNow.AddDays(-30), TotalAmount = 500.00m, DiscountAmount = 25.00m, OrderStatus = OrderStatus.Delivered, DeliveredDate = DateTime.UtcNow.AddDays(-28) },
                new Order { Id = 202, CustomerId = 2, OrderDate = DateTime.UtcNow.AddDays(-25), TotalAmount = 120.75m, DiscountAmount = 6.00m, OrderStatus = OrderStatus.Delivered, DeliveredDate = DateTime.UtcNow.AddDays(-24) },
                new Order { Id = 203, CustomerId = 2, OrderDate = DateTime.UtcNow.AddDays(-20), TotalAmount = 300.00m, DiscountAmount = 15.00m, OrderStatus = OrderStatus.Delivered, DeliveredDate = DateTime.UtcNow.AddDays(-19) },
                new Order { Id = 204, CustomerId = 2, OrderDate = DateTime.UtcNow.AddDays(-15), TotalAmount = 80.00m, DiscountAmount = 4.00m, OrderStatus = OrderStatus.Delivered, DeliveredDate = DateTime.UtcNow.AddDays(-14) },
                new Order { Id = 205, CustomerId = 2, OrderDate = DateTime.UtcNow.AddDays(-10), TotalAmount = 180.00m, DiscountAmount = 9.00m, OrderStatus = OrderStatus.Delivered, DeliveredDate = DateTime.UtcNow.AddDays(-9) },
                new Order { Id = 206, CustomerId = 2, OrderDate = DateTime.UtcNow.AddDays(-2), TotalAmount = 75.00m, DiscountAmount = 0.00m, OrderStatus = OrderStatus.Pending },

                new Order { Id = 301, CustomerId = 3, OrderDate = DateTime.UtcNow.AddDays(-10), TotalAmount = 99.99m, DiscountAmount = 0.00m, OrderStatus = OrderStatus.Shipped },
                new Order { Id = 302, CustomerId = 3, OrderDate = DateTime.UtcNow.AddDays(-7), TotalAmount = 45.00m, DiscountAmount = 0.00m, OrderStatus = OrderStatus.Cancelled },

                new Order { Id = 401, CustomerId = 4, OrderDate = DateTime.UtcNow.AddDays(-18), TotalAmount = 220.00m, DiscountAmount = 11.00m, OrderStatus = OrderStatus.Delivered, DeliveredDate = DateTime.UtcNow.AddDays(-17) },
                new Order { Id = 402, CustomerId = 4, OrderDate = DateTime.UtcNow.AddDays(-12), TotalAmount = 60.00m, DiscountAmount = 0.00m, OrderStatus = OrderStatus.Processing }
            );
        }
    }
}