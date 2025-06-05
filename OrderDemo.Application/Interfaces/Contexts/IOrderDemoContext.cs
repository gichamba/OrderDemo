using Microsoft.EntityFrameworkCore;
using OrderDemo.Domain.Entities;


namespace OrderDemo.Application.Interfaces.Contexts {
    /// <summary>
    /// Defines the contract for the application's database context,
    /// abstracting the underlying data persistence mechanism.
    /// </summary>
    public interface IOrderDemoContext : IDisposable, IAsyncDisposable {
        /// <summary>
        /// Gets a DbSet for managing Customer entities.
        /// </summary>
        DbSet<Customer> Customers { get; }

        /// <summary>
        /// Gets a DbSet for managing Order entities.
        /// </summary>
        DbSet<Order> Orders { get; }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}