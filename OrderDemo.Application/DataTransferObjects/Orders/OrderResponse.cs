using OrderDemo.Domain.Enums; // Reference to your domain enums

namespace OrderDemo.Application.DataTransferObjects.Orders {
    /// <summary>
    /// Represents the detailed information of an order returned to the client.
    /// </summary>
    public class OrderResponse {
        /// <summary>
        /// The unique identifier of the order.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The date and time when the order was placed.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// The total amount of the order before any discounts.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// The amount discounted from the order total.
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// The final amount of the order after applying discounts.
        /// </summary>
        public decimal FinalAmount { get; set; }

        /// <summary>
        /// The current status of the order.
        /// </summary>
        public OrderStatus OrderStatus { get; set; }

        /// <summary>
        /// The date and time when the order was delivered. Null if not yet delivered.
        /// </summary>
        public DateTime? DeliveredDate { get; set; }

        /// <summary>
        /// The unique identifier of the customer who placed the order.
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// The name of the customer who placed the order.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// The customer segment of the customer who placed the order.
        /// </summary>
        public CustomerSegment CustomerSegment { get; set; }
    }
}