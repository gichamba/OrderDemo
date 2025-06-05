namespace OrderDemo.Domain.Enums {
    public enum CustomerSegment {
        /// <summary>
        /// Represents a new customer with no prior orders.
        /// </summary>
        New = 0,

        /// <summary>
        /// Represents a loyal customer with a significant order history.
        /// </summary>
        Loyal = 1,

        /// <summary>
        /// Represents a wholesale customer, potentially with different pricing structures.
        /// </summary>
        Wholesale = 2,

        /// <summary>
        /// Represents a regular customer who doesn't fall into other specific segments.
        /// </summary>
        Regular = 3
    }
}