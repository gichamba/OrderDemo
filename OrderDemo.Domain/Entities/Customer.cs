using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OrderDemo.Domain.Enums;

namespace OrderDemo.Domain.Entities {
    /// <summary>
    /// Represents a customer in the order management system.
    /// </summary>
    public class Customer {
        #region Primitive Properties
        /// <summary>
        /// Unique identifier for the customer.
        /// </summary>

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The name of the customer.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; 

        /// <summary>
        /// The segment to which the customer belongs, used for discounting and promotions.
        /// </summary>
        [Required]
        public CustomerSegment CustomerSegment { get; set; }
        #endregion Primitive Properties

        #region Navigation Properties
        #region Children
        /// <summary>
        /// Navigation property for orders placed by this customer.
        /// </summary>
        [InverseProperty(nameof(Order.Customer))]
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        #endregion Children
        #endregion Navigation Properties
    }
}