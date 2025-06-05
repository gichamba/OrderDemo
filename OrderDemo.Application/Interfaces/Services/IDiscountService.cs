using OrderDemo.Domain.Entities;

namespace OrderDemo.Application.Interfaces.Services {
    /// <summary>
    /// Defines the contract for applying and managing discounts.
    /// </summary>
    public interface IDiscountService {
        /// <summary>
        /// Calculates and applies a discount to an order based on customer segment and order history.
        /// The discount amount is updated directly on the provided Order entity.
        /// </summary>
        /// <param name="order">The Order entity to which the discount should be applied.</param>
        /// <param name="customer">The Customer entity associated with the order.</param>
        /// <returns>The calculated discount amount.</returns>
        decimal ApplyDiscount(Order order, Customer customer);
    }
}