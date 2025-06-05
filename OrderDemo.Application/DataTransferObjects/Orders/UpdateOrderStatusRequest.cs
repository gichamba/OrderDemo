using System.ComponentModel.DataAnnotations;
using OrderDemo.Domain.Enums;

namespace OrderDemo.Application.DataTransferObjects.Orders {
    /// <summary>
    /// Represents the data required to update the status of an existing order.
    /// </summary>
    public class UpdateOrderStatusRequest {
        /// <summary>
        /// The unique identifier of the order to be updated.
        /// </summary>
        [Required(ErrorMessage = "Order ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Order ID must be a positive integer.")]
        public int OrderId { get; set; }

        /// <summary>
        /// The new status to set for the order.
        /// </summary>
        [Required(ErrorMessage = "New status is required.")]
        [EnumDataType(typeof(OrderStatus), ErrorMessage = "Invalid order status value.")]
        public OrderStatus NewStatus { get; set; }
    }
}