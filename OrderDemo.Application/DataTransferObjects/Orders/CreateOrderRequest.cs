using System.ComponentModel.DataAnnotations;

namespace OrderDemo.Application.DataTransferObjects.Orders {
    /// <summary>
    /// Represents the data required to create a new order.
    /// </summary>
    public class CreateOrderRequest {
        /// <summary>
        /// The unique identifier of the customer placing the order.
        /// </summary>
        [Required(ErrorMessage = "Customer ID is required.")]
        public int CustomerId { get; set; }

        /// <summary>
        /// The total amount of the order before any discounts.
        /// </summary>
        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
        public decimal TotalAmount { get; set; }

        // Order items are omitted as they are not required for this assessment.
    }
}