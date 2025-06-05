using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OrderDemo.Domain.Enums;

namespace OrderDemo.Domain.Entities {
    /// <summary>
    /// Represents an order placed by a customer.
    /// </summary>
    public class Order {
        #region Primitive Properties
        /// <summary>
        /// Unique identifier for the order.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The date and time when the order was placed.
        /// </summary>
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; 

        /// <summary>
        /// The total amount of the order before any discounts.
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// The amount discounted from the order total.
        /// </summary>
        [Required]
        [Range(0.00, double.MaxValue, ErrorMessage = "Discount amount cannot be negative.")]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal DiscountAmount { get; set; } = 0.00m; 

        /// <summary>
        /// The final amount after applying discounts.
        /// </summary>
        [NotMapped] 
        public decimal FinalAmount => TotalAmount - DiscountAmount;

        /// <summary>
        /// The current status of the order.
        /// </summary>
        [Required]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending; 

        /// <summary>
        /// The date and time when the order was delivered. Null until delivered.
        /// </summary>
        public DateTime? DeliveredDate { get; set; }
        #endregion Primitive Properties

        #region Foreign Keys
        /// <summary>
        /// Foreign key to the Customer who placed this order.
        /// </summary>
        [Required]
        public int CustomerId { get; set; }
        #endregion Foreign Keys

        #region Navigation Properties
        #region Parents
        /// <summary>
        /// Navigation property to the Customer who placed this order.
        /// </summary>
        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; } = default!; 
        #endregion Parents
        #endregion Navigation Properties
    }
}