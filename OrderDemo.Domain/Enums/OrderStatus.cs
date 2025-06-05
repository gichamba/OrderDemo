

namespace OrderDemo.Domain.Enums;

    public enum OrderStatus {
        /// <summary>
        /// The order has been placed but not yet processed.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The order is currently being prepared or processed.
        /// </summary>
        Processing = 1,

        /// <summary>
        /// The order has been shipped to the customer.
        /// </summary>
        Shipped = 2,

        /// <summary>
        /// The order has been successfully delivered to the customer.
        /// </summary>
        Delivered = 3,

        /// <summary>
        /// The order has been cancelled.
        /// </summary>
        Cancelled = 4
    }

