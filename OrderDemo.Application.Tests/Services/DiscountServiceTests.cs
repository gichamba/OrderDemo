using FluentAssertions;
using OrderDemo.Application.Interfaces.Services;
using OrderDemo.Application.Services;
using OrderDemo.Domain.Entities;
using OrderDemo.Domain.Enums;


namespace OrderDemo.Application.Tests.Services {
    public class DiscountServiceTests {
        private readonly IDiscountService _discountService = new DiscountService();

        #region Input Validation Tests

        [Fact]
        public void ApplyDiscount_ShouldThrowArgumentNullException_WhenOrderIsNull() {
            // Arrange
            Order order = null;
            Customer customer = new() { Id = 1, CustomerSegment = CustomerSegment.New };

            // Act
            Action act = () => _discountService.ApplyDiscount(order, customer);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("order");
        }

        [Fact]
        public void ApplyDiscount_ShouldThrowArgumentNullException_WhenCustomerIsNull() {
            // Arrange
            Order order = new() { Id = 1, TotalAmount = 100m };
            Customer customer = null;

            // Act
            Action act = () => _discountService.ApplyDiscount(order, customer);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("customer");
        }

        [Fact]
        public void ApplyDiscount_ShouldReturnZeroDiscount_WhenTotalAmountIsZero() {
            // Arrange
            Customer customer = new() { CustomerSegment = CustomerSegment.Loyal, Orders = new List<Order>(Enumerable.Repeat(new Order { OrderStatus = OrderStatus.Delivered }, 5)) };
            Order order = new() { Id = 1, TotalAmount = 0.00m };
            decimal expectedDiscount = 0.00m;

            // Act
            decimal actualDiscount = _discountService.ApplyDiscount(order, customer);

            // Assert
            actualDiscount.Should().Be(expectedDiscount);
            order.DiscountAmount.Should().Be(expectedDiscount);
        }

        [Fact]
        public void ApplyDiscount_ShouldReturnZeroDiscount_WhenTotalAmountIsNegative() {
            // Arrange
            Customer customer = new() { CustomerSegment = CustomerSegment.Loyal, Orders = new List<Order>(Enumerable.Repeat(new Order { OrderStatus = OrderStatus.Delivered }, 5)) };
            Order order = new() { Id = 1, TotalAmount = -50.00m };
            decimal expectedDiscount = 0.00m;

            // Act
            decimal actualDiscount = _discountService.ApplyDiscount(order, customer);

            // Assert
            actualDiscount.Should().Be(expectedDiscount);
            order.DiscountAmount.Should().Be(expectedDiscount);
        }

        #endregion

        #region New Customer Discount Tests

        [Fact]
        public void ApplyDiscount_NewCustomer_IsFirstOrder_ShouldReturnTenPercentDiscount() {
            // Arrange
            // Customer has no existing orders, so the current one is their first.
            Customer customer = new() { Id = 1, CustomerSegment = CustomerSegment.New, Orders = new List<Order>() };
            Order order = new() { Id = 100, TotalAmount = 200.00m };
            decimal expectedDiscount = 200.00m * 0.10m;

            // Act
            decimal actualDiscount = _discountService.ApplyDiscount(order, customer);

            // Assert
            actualDiscount.Should().Be(expectedDiscount);
            order.DiscountAmount.Should().Be(expectedDiscount);
        }

        [Fact]
        public void ApplyDiscount_NewCustomer_IsNotFirstOrder_ShouldReturnZeroDiscount() {
            // Arrange
            // Customer has one existing order, so the current one is not their first.
            Customer customer = new() {
                Id = 1,
                CustomerSegment = CustomerSegment.New,
                Orders = new List<Order> { new() { Id = 1, OrderStatus = OrderStatus.Delivered } }
            };
            Order order = new() { Id = 100, TotalAmount = 200.00m };

            decimal expectedDiscount = 0.00m;

            // Act
            decimal actualDiscount = _discountService.ApplyDiscount(order, customer);

            // Assert
            actualDiscount.Should().Be(expectedDiscount);
            order.DiscountAmount.Should().Be(expectedDiscount);
        }

        #endregion

        #region Loyal Customer Discount Tests

        [Fact]
        public void ApplyDiscount_LoyalCustomer_HasFiveOrMoreDeliveredOrders_ShouldReturnFivePercentDiscount() {
            // Arrange
            Customer customer = new() {
                Id = 1,
                CustomerSegment = CustomerSegment.Loyal,
                Orders = new List<Order>(Enumerable.Repeat(new Order { OrderStatus = OrderStatus.Delivered }, 5))
            };
            Order order = new() { Id = 100, TotalAmount = 100.00m };
            decimal expectedDiscount = 100.00m * 0.05m;

            // Act
            decimal actualDiscount = _discountService.ApplyDiscount(order, customer);

            // Assert
            actualDiscount.Should().Be(expectedDiscount);
            order.DiscountAmount.Should().Be(expectedDiscount);
        }

        [Fact]
        public void ApplyDiscount_LoyalCustomer_HasLessThanFiveDeliveredOrders_ShouldReturnZeroDiscount() {
            // Arrange
            Customer customer = new() {
                Id = 1,
                CustomerSegment = CustomerSegment.Loyal,
                Orders = new List<Order>
                {
                    new() { OrderStatus = OrderStatus.Delivered },
                    new() { OrderStatus = OrderStatus.Delivered },
                    new() { OrderStatus = OrderStatus.Delivered },
                    new() { OrderStatus = OrderStatus.Delivered },
                    new() { OrderStatus = OrderStatus.Pending }
                }
            };
            Order order = new() { Id = 100, TotalAmount = 100.00m };
            decimal expectedDiscount = 0.00m;

            // Act
            decimal actualDiscount = _discountService.ApplyDiscount(order, customer);

            // Assert
            actualDiscount.Should().Be(expectedDiscount);
            order.DiscountAmount.Should().Be(expectedDiscount);
        }

        [Fact]
        public void ApplyDiscount_LoyalCustomer_NoDeliveredOrders_ShouldReturnZeroDiscount() {
            // Arrange
            Customer customer = new() { Id = 1, CustomerSegment = CustomerSegment.Loyal, Orders = new List<Order>() };
            Order order = new() { Id = 100, TotalAmount = 100.00m };
            decimal expectedDiscount = 0.00m;

            // Act
            decimal actualDiscount = _discountService.ApplyDiscount(order, customer);

            // Assert
            actualDiscount.Should().Be(expectedDiscount);
            order.DiscountAmount.Should().Be(expectedDiscount);
        }

        #endregion

        #region Regular and Wholesale Customer Discount Tests

        [Theory]
        [InlineData(CustomerSegment.Regular)]
        [InlineData(CustomerSegment.Wholesale)]
        public void ApplyDiscount_RegularOrWholesaleCustomer_ShouldReturnZeroDiscount(CustomerSegment segment) {
            // Arrange
            Customer customer = new() { Id = 1, CustomerSegment = segment, Orders = new List<Order> { new() { OrderStatus = OrderStatus.Delivered } } };
            Order order = new() { Id = 100, TotalAmount = 100.00m };
            decimal expectedDiscount = 0.00m;

            // Act
            decimal actualDiscount = _discountService.ApplyDiscount(order, customer);

            // Assert
            actualDiscount.Should().Be(expectedDiscount);
            order.DiscountAmount.Should().Be(expectedDiscount);
        }

        #endregion
    }
}