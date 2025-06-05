namespace OrderDemo.Application.DataTransferObjects.Analytics {
    /// <summary>
    /// Represents aggregated analytical data for orders.
    /// </summary>
    public class OrderAnalyticsDto {
        /// <summary>
        /// The average monetary value of all orders.
        /// </summary>
        public decimal AverageOrderValue { get; set; }

        /// <summary>
        /// The average time taken to fulfill (deliver) an order, in hours.
        /// </summary>
        public double AverageFulfillmentTimeInHours { get; set; }

        /// <summary>
        /// The total number of orders in the system.
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// The total number of orders currently in a 'Pending' status.
        /// </summary>
        public int TotalPendingOrders { get; set; }

        /// <summary>
        /// The total number of orders that have been successfully 'Delivered'.
        /// </summary>
        public int TotalDeliveredOrders { get; set; }
    }
}