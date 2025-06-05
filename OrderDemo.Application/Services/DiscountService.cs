using OrderDemo.Application.Interfaces.Services;
using OrderDemo.Domain.Entities;
using OrderDemo.Domain.Enums;

namespace OrderDemo.Application.Services {
    /// <summary>
    /// Provides services for applying discounts based on customer segments and order history.
    /// </summary>
    public class DiscountService : IDiscountService {

        /// <inheritdoc />
        public decimal ApplyDiscount(Order order, Customer customer) {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (customer == null) throw new ArgumentNullException(nameof(customer));

            decimal discountPercentage = 0.00m;

            switch (customer.CustomerSegment) {
                case CustomerSegment.New:
                    // Apply 10% discount for new customers on their first order.
                    // This assumes the 'Orders' navigation property on Customer is loaded.
                    if (customer.Orders.Count == 0) {
                        discountPercentage = 0.10m; // 10% discount
                    }
                    break;
                case CustomerSegment.Loyal:
                    // Apply 5% discount for loyal customers with 5 or more delivered orders.
                    if (customer.Orders.Count(o => o.OrderStatus == OrderStatus.Delivered) >= 5) {
                        discountPercentage = 0.05m; // 5% discount
                    }
                    break;
                case CustomerSegment.Wholesale:
                    // No automatic discount for wholesale customers in this stage.
                    break;
                case CustomerSegment.Regular:
                    // No automatic discount for regular customers in this stage.
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            decimal calculatedDiscount = order.TotalAmount * discountPercentage;

            // Ensure the discount is not negative and does not exceed the total amount.
            order.DiscountAmount = Math.Max(0, Math.Min(calculatedDiscount, order.TotalAmount));

            return order.DiscountAmount;
        }
    }
}