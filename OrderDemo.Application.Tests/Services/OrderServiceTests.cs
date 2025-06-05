using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OneOf;
using OrderDemo.Application.Common.Results;
using OrderDemo.Application.DataTransferObjects.Orders;
using OrderDemo.Application.Interfaces.Contexts;
using OrderDemo.Application.Interfaces.Services;
using OrderDemo.Application.Services;
using OrderDemo.Domain.Entities;
using OrderDemo.Domain.Enums;
using System.Linq.Expressions;


namespace OrderDemo.Application.Tests.Services {
    public class OrderServiceTests {
        private readonly Mock<IOrderDemoContext> _mockContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IDiscountService> _mockDiscountService;
        private readonly OrderService _orderService;

        public OrderServiceTests() {
            _mockContext = new Mock<IOrderDemoContext>();
            _mockMapper = new Mock<IMapper>();
            _mockDiscountService = new Mock<IDiscountService>();

            _orderService = new OrderService(
                _mockContext.Object,
                _mockMapper.Object,
                _mockDiscountService.Object
            );
        }

        #region CreateOrderAsync Tests

        [Fact]
        public async Task CreateOrderAsync_ShouldReturnSuccess_WhenOrderIsCreatedSuccessfully() {
            // Arrange
            CreateOrderRequest request = new() { CustomerId = 1, TotalAmount = 100.00m };

            Customer customer = new() { Id = 1, Name = "Test Customer", CustomerSegment = CustomerSegment.New };
            Order order = new() { CustomerId = 1, TotalAmount = 100.00m, OrderDate = DateTime.UtcNow, OrderStatus = OrderStatus.Pending };
            OrderResponse orderResponse = new() { Id = 1, CustomerId = 1, CustomerName = "Test Customer", TotalAmount = 100.00m, FinalAmount = 100.00m };

            _mockContext.Setup(c => c.Customers).Returns(GetQueryableMockDbSet([customer]));
            _mockMapper.Setup(m => m.Map<Order>(request)).Returns(order);
            _mockDiscountService.Setup(s => s.ApplyDiscount(order, customer)).Returns(0m);
            _mockContext.Setup(c => c.Orders.Add(It.IsAny<Order>()));
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<OrderResponse>(order)).Returns(orderResponse);

            // Act
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await _orderService.CreateOrderAsync(request);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>>();
            result.AsT0.Data.Should().Be(orderResponse);
            _mockContext.Verify(c => c.Orders.Add(order), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldReturnValidationFailure_WhenRequestIsInvalid() {
            // Arrange
            CreateOrderRequest request = new() { CustomerId = 0, TotalAmount = 0.00m };

            // Act
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await _orderService.CreateOrderAsync(request);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>>();
            result.AsT1.Should().BeOfType<ValidationFailure>();
            result.AsT1.Errors.Should().NotBeEmpty();
            result.AsT1.Errors.Should().Contain(e => e.Message.Contains("Total amount must be greater than zero."));
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldReturnNotFound_WhenCustomerDoesNotExist() {
            // Arrange
            CreateOrderRequest request = new() { CustomerId = 999, TotalAmount = 100.00m };

            _mockContext.Setup(c => c.Customers).Returns(GetQueryableMockDbSet(new List<Customer>()));

            // Act
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await _orderService.CreateOrderAsync(request);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>>();
            result.AsT2.Should().BeOfType<NotFound>();
            result.AsT2.Message.Should().Contain("Customer with ID '999' not found.");
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldReturnUnexpectedError_WhenDatabaseOperationFails() {
            // Arrange
            CreateOrderRequest request = new() { CustomerId = 1, TotalAmount = 100.00m };
            Customer customer = new() { Id = 1, Name = "Test Customer", CustomerSegment = CustomerSegment.New };
            Order order = new() { CustomerId = 1, TotalAmount = 100.00m };

            _mockContext.Setup(c => c.Customers).Returns(GetQueryableMockDbSet([customer]));

            _mockMapper.Setup(m => m.Map<Order>(request)).Returns(order);
            _mockDiscountService.Setup(s => s.ApplyDiscount(order, customer)).Returns(0m);
            _mockContext.Setup(c => c.Orders.Add(It.IsAny<Order>()));
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateException("Simulated DB error.", (Exception?)null));

            // Act
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await _orderService.CreateOrderAsync(request);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>>();
            result.AsT3.Should().BeOfType<UnexpectedError>();
            result.AsT3.Message.Should().Contain("A database error occurred");
        }

        #endregion

        #region GetOrderByIdAsync Tests

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnSuccess_WhenOrderExists() {
            // Arrange
            int orderId = 1;
            Customer customer = new() { Id = 1, Name = "Test Customer", CustomerSegment = CustomerSegment.Regular };
            Order order = new() { Id = orderId, CustomerId = 1, Customer = customer, TotalAmount = 50.00m };
            OrderResponse orderResponse = new() { Id = orderId, CustomerId = 1, CustomerName = "Test Customer", TotalAmount = 50.00m };

            _mockContext.Setup(c => c.Orders)
                        .Returns(GetQueryableMockDbSet([order]));

            _mockMapper.Setup(m => m.Map<OrderResponse>(order)).Returns(orderResponse);

            // Act
            OneOf<Success<OrderResponse>, NotFound, UnexpectedError> result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, NotFound, UnexpectedError>>();
            result.AsT0.Data.Should().Be(orderResponse);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnNotFound_WhenOrderDoesNotExist() {
            // Arrange
            int orderId = 999;
            _mockContext.Setup(c => c.Orders)
                        .Returns(GetQueryableMockDbSet(new List<Order>()));

            // Act
            OneOf<Success<OrderResponse>, NotFound, UnexpectedError> result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, NotFound, UnexpectedError>>();
            result.AsT1.Should().BeOfType<NotFound>();
            result.AsT1.Message.Should().Contain($"Order with ID '{orderId}' not found.");
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnUnexpectedError_WhenDatabaseOperationFails() {
            // Arrange
            int orderId = 1;
            _mockContext.Setup(c => c.Orders).Throws(new InvalidOperationException("Simulated DB connection error."));

            // Act
            OneOf<Success<OrderResponse>, NotFound, UnexpectedError> result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, NotFound, UnexpectedError>>();
            result.AsT2.Should().BeOfType<UnexpectedError>();
            result.AsT2.Message.Should().Contain("Simulated DB connection error.");
        }

        #endregion

        #region GetAllOrdersAsync Tests

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnSuccessWithOrders_WhenOrdersExist() {
            // Arrange
            Customer customer1 = new() { Id = 1, Name = "Customer A", CustomerSegment = CustomerSegment.Regular };
            Customer customer2 = new() { Id = 2, Name = "Customer B", CustomerSegment = CustomerSegment.Loyal };
            List<Order> orders = [
                new Order { Id = 1, CustomerId = 1, Customer = customer1, TotalAmount = 100m },
                new Order { Id = 2, CustomerId = 2, Customer = customer2, TotalAmount = 200m }
            ];
            List<OrderResponse> orderResponses = [
                new OrderResponse { Id = 1, CustomerId = 1, CustomerName = "Customer A", TotalAmount = 100m },
                new OrderResponse { Id = 2, CustomerId = 2, CustomerName = "Customer B", TotalAmount = 200m }
            ];

            _mockContext.Setup(c => c.Orders).Returns(GetQueryableMockDbSet(orders));
            _mockMapper.Setup(m => m.Map<OrderResponse>(orders[0])).Returns(orderResponses[0]);
            _mockMapper.Setup(m => m.Map<OrderResponse>(orders[1])).Returns(orderResponses[1]);

            // Act
            OneOf<Success<IEnumerable<OrderResponse>>, UnexpectedError> result = await _orderService.GetAllOrdersAsync();

            // Assert
            result.Should().BeOfType<OneOf<Success<IEnumerable<OrderResponse>>, UnexpectedError>>();
            result.AsT0.Data.Should().BeEquivalentTo(orderResponses);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnSuccessWithEmptyList_WhenNoOrdersExist() {
            // Arrange
            _mockContext.Setup(c => c.Orders)
                        .Returns(GetQueryableMockDbSet(new List<Order>()));

            // Act
            OneOf<Success<IEnumerable<OrderResponse>>, UnexpectedError> result = await _orderService.GetAllOrdersAsync();

            // Assert
            result.Should().BeOfType<OneOf<Success<IEnumerable<OrderResponse>>, UnexpectedError>>();
            result.AsT0.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnUnexpectedError_WhenDatabaseOperationFails() {
            // Arrange
            _mockContext.Setup(c => c.Orders).Throws(new DbUpdateException("Simulated DB error.", (Exception?)null));

            // Act
            OneOf<Success<IEnumerable<OrderResponse>>, UnexpectedError> result = await _orderService.GetAllOrdersAsync();

            // Assert
            result.Should().BeOfType<OneOf<Success<IEnumerable<OrderResponse>>, UnexpectedError>>();
            result.AsT1.Should().BeOfType<UnexpectedError>();
            result.AsT1.Message.Should().Contain("A database error occurred");
        }

        #endregion

        #region UpdateOrderStatusAsync Tests

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldReturnSuccess_WhenStatusIsUpdatedSuccessfully() {
            // Arrange
            int orderId = 1;
            Customer customer = new() { Id = 1, Name = "Test Customer" };
            Order existingOrder = new() { Id = orderId, CustomerId = 1, Customer = customer, OrderStatus = OrderStatus.Pending };
            UpdateOrderStatusRequest request = new() { OrderId = orderId, NewStatus = OrderStatus.Processing };
            OrderResponse updatedOrderResponse = new() { Id = orderId, CustomerId = 1, CustomerName = "Test Customer", OrderStatus = OrderStatus.Processing };

            _mockContext.Setup(c => c.Orders).Returns(GetQueryableMockDbSet([existingOrder]));

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<OrderResponse>(It.Is<Order>(o => o.OrderStatus == OrderStatus.Processing && o.Id == orderId)))
                       .Returns(updatedOrderResponse);

            // Act
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await _orderService.UpdateOrderStatusAsync(request);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>>();
            result.AsT0.Data.Should().Be(updatedOrderResponse);
            existingOrder.OrderStatus.Should().Be(OrderStatus.Processing);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldReturnValidationFailure_WhenOrderIsAlreadyInNewStatus() {
            // Arrange
            int orderId = 1;
            Customer customer = new() { Id = 1, Name = "Test Customer" };
            Order existingOrder = new() { Id = orderId, CustomerId = 1, Customer = customer, OrderStatus = OrderStatus.Pending };
            UpdateOrderStatusRequest request = new() { OrderId = orderId, NewStatus = OrderStatus.Pending };

            _mockContext.Setup(c => c.Orders)
                        .Returns(GetQueryableMockDbSet([existingOrder]));

            // Act
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await _orderService.UpdateOrderStatusAsync(request);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>>();
            result.AsT1.Should().BeOfType<ValidationFailure>();
            result.AsT1.Errors.Should().Contain(e => e.Message.Contains("Order is already in 'Pending' status."));
        }

        [Theory]
        [InlineData(OrderStatus.Processing, OrderStatus.Pending)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Processing)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Pending)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Delivered)]
        public async Task UpdateOrderStatusAsync_ShouldReturnValidationFailure_WhenTransitionIsInvalid(OrderStatus currentStatus, OrderStatus newStatus) {
            // Arrange
            int orderId = 1;
            Customer customer = new() { Id = 1, Name = "Test Customer" };
            Order existingOrder = new() { Id = orderId, CustomerId = 1, Customer = customer, OrderStatus = currentStatus };
            UpdateOrderStatusRequest request = new() { OrderId = orderId, NewStatus = newStatus };

            _mockContext.Setup(c => c.Orders).Returns(GetQueryableMockDbSet([existingOrder]));

            // Act
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await _orderService.UpdateOrderStatusAsync(request);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>>();
            result.AsT1.Should().BeOfType<ValidationFailure>();
            result.AsT1.Errors.Should().Contain(e => e.Message.Contains($"Invalid status transition from '{currentStatus}' to '{newStatus}'."));
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldSetDeliveredDate_WhenTransitionToDelivered() {
            // Arrange
            int orderId = 1;
            Customer customer = new() { Id = 1, Name = "Test Customer" };
            Order existingOrder = new() { Id = orderId, CustomerId = 1, Customer = customer, OrderStatus = OrderStatus.Shipped, DeliveredDate = null };
            UpdateOrderStatusRequest request = new() { OrderId = orderId, NewStatus = OrderStatus.Delivered };

            _mockContext.Setup(c => c.Orders).Returns(GetQueryableMockDbSet([existingOrder]));

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<OrderResponse>(It.IsAny<Order>())).Returns((Order o) => new OrderResponse { Id = o.Id, CustomerId = o.CustomerId, CustomerName = o.Customer?.Name, OrderStatus = o.OrderStatus, DeliveredDate = o.DeliveredDate });


            // Act
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await _orderService.UpdateOrderStatusAsync(request);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>>();
            result.AsT0.Data.OrderStatus.Should().Be(OrderStatus.Delivered);
            result.AsT0.Data.DeliveredDate.Should().HaveValue();
            result.AsT0.Data.DeliveredDate.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldReturnNotFound_WhenOrderDoesNotExist() {
            // Arrange
            int orderId = 999;
            UpdateOrderStatusRequest request = new() { OrderId = orderId, NewStatus = OrderStatus.Shipped };

            _mockContext.Setup(c => c.Orders)
                        .Returns(GetQueryableMockDbSet(new List<Order>()));

            // Act
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await _orderService.UpdateOrderStatusAsync(request);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>>();
            result.AsT2.Should().BeOfType<NotFound>();
            result.AsT2.Message.Should().Contain($"Order with ID '{orderId}' not found.");
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ShouldReturnUnexpectedError_WhenDatabaseOperationFails() {
            // Arrange
            int orderId = 1;
            Customer customer = new() { Id = 1, Name = "Test Customer" };
            Order existingOrder = new() { Id = orderId, CustomerId = 1, Customer = customer, OrderStatus = OrderStatus.Pending };
            UpdateOrderStatusRequest request = new() { OrderId = orderId, NewStatus = OrderStatus.Processing };

            _mockContext.Setup(c => c.Orders)
                        .Returns(GetQueryableMockDbSet([existingOrder]));

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DbUpdateException("Simulated DB error.", (Exception?)null));

            // Act
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await _orderService.UpdateOrderStatusAsync(request);

            // Assert
            result.Should().BeOfType<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>>();
            result.AsT3.Should().BeOfType<UnexpectedError>();
            result.AsT3.Message.Should().Contain("A database error occurred");
        }

        #endregion


        private static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class {
            IQueryable<T> queryable = sourceList.AsQueryable();
            Mock<DbSet<T>> mockDbSet = new();

            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            mockDbSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
            return mockDbSet.Object;
        }


        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider(IQueryProvider inner) {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression) {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object? Execute(Expression expression) {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression) {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default) {
                Type expectedResultType = typeof(TResult).GetGenericArguments()[0];
                if (expectedResultType == typeof(TEntity)) {
                    TEntity result = Execute<TEntity>(expression);
                    return (TResult)(object)Task.FromResult(result);
                }

                if (expectedResultType.GetGenericArguments().FirstOrDefault() == typeof(TEntity)) {
                    IEnumerable<TEntity>? result = (IEnumerable<TEntity>)_inner.Execute(expression);
                    return (TResult)(object)Task.FromResult(result.ToList());
                }
                return Execute<TResult>(expression);
            }
        }

        private class TestAsyncEnumerable<T>(Expression expression) : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T> {

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        private class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T> {
            public T Current => inner.Current;

            public ValueTask<bool> MoveNextAsync() {
                return new ValueTask<bool>(inner.MoveNext());
            }

            public ValueTask DisposeAsync() {
                inner.Dispose();
                return new ValueTask();
            }
        }
    }
}