using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using OrderDemo.Application.DataTransferObjects.Orders;
using OrderDemo.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrderDemo.WebApi.IntegrationTests.Configuration;
using Xunit;

namespace OrderDemo.WebApi.IntegrationTests.Endpoints {
    public class OrderEndpointsTests(CustomWebApplicationFactory<Program> factory)
        : IClassFixture<CustomWebApplicationFactory<Program>> {
        private readonly HttpClient _client = factory.CreateClient();
        private readonly ILogger<OrderEndpointsTests> _logger = factory.Services.GetRequiredService<ILogger<OrderEndpointsTests>>();

        [Fact]
        public async Task CreateOrder_ValidRequest_ReturnsCreatedOrder() {
            // Arrange
            CreateOrderRequest request = new() {
                CustomerId = 1,
                TotalAmount = 100.00m
            };

            _logger.LogDebug("Test: CreateOrder_ValidRequest_ReturnsCreatedOrder");
            _logger.LogDebug($"Request URL: {_client.BaseAddress}orders/");

            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            OrderResponse? createdOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
            _logger.LogDebug($"Response Status Code: {response.StatusCode}");
            _logger.LogDebug($"Response Content: {await response.Content.ReadAsStringAsync()}");

            createdOrder.Should().NotBeNull();
            createdOrder!.CustomerId.Should().Be(request.CustomerId);
            createdOrder.TotalAmount.Should().Be(request.TotalAmount);
            createdOrder.DiscountAmount.Should().Be(10.00m, "because Alice is a new customer with her first order.");
            createdOrder.FinalAmount.Should().Be(90.00m);
            createdOrder.CustomerSegment.Should().Be(CustomerSegment.New);
        }

        [Fact]
        public async Task CreateOrder_NewCustomerFirstOrder_AppliesTenPercentDiscount() {
            // Arrange
            CreateOrderRequest request = new() {
                CustomerId = 5,
                TotalAmount = 100.00m
            };

            _logger.LogDebug("Test: CreateOrder_NewCustomerFirstOrder_AppliesTenPercentDiscount");
            _logger.LogDebug($"Request URL: {_client.BaseAddress}orders/");

            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            OrderResponse? createdOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
            _logger.LogDebug($"Response Status Code: {response.StatusCode}");
            _logger.LogDebug($"Response Content: {await response.Content.ReadAsStringAsync()}");

            createdOrder.Should().NotBeNull();
            createdOrder!.CustomerId.Should().Be(request.CustomerId);
            createdOrder.TotalAmount.Should().Be(request.TotalAmount);
            createdOrder.DiscountAmount.Should().Be(10.00m, "because a new customer's first order should get a 10% discount.");
            createdOrder.FinalAmount.Should().Be(90.00m);
            createdOrder.CustomerSegment.Should().Be(CustomerSegment.New);
        }

        [Fact]
        public async Task CreateOrder_LoyalCustomerWithEnoughOrders_AppliesFivePercentDiscount() {
            // Arrange
            CreateOrderRequest request = new() {
                CustomerId = 2,
                TotalAmount = 100.00m
            };

            _logger.LogDebug("Test: CreateOrder_LoyalCustomerWithEnoughOrders_AppliesFivePercentDiscount");
            _logger.LogDebug($"Request URL: {_client.BaseAddress}orders/");

            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            OrderResponse? createdOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
            _logger.LogDebug($"Response Status Code: {response.StatusCode}");
            _logger.LogDebug($"Response Content: {await response.Content.ReadAsStringAsync()}");

            createdOrder.Should().NotBeNull();
            createdOrder!.CustomerId.Should().Be(request.CustomerId);
            createdOrder.TotalAmount.Should().Be(request.TotalAmount);
            createdOrder.DiscountAmount.Should().Be(5.00m, "because a loyal customer with enough orders should get a 5% discount.");
            createdOrder.FinalAmount.Should().Be(95.00m);
            createdOrder.CustomerSegment.Should().Be(CustomerSegment.Loyal);
        }

        [Fact]
        public async Task CreateOrder_InvalidTotalAmount_ReturnsBadRequest() {
            // Arrange
            CreateOrderRequest request = new() {
                CustomerId = 1,
                TotalAmount = 0.00m
            };

            _logger.LogDebug("Test: CreateOrder_InvalidTotalAmount_ReturnsBadRequest");
            _logger.LogDebug($"Request URL: {_client.BaseAddress}orders/");

            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            string content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"Response Status Code: {response.StatusCode}");
            _logger.LogDebug($"Response Content: {content}");
            content.Should().Contain("Total amount must be greater than zero.");
        }

        [Fact]
        public async Task CreateOrder_InvalidCustomerId_ReturnsNotFound() {
            // Arrange
            CreateOrderRequest request = new() {
                CustomerId = 999,
                TotalAmount = 50.00m
            };

            _logger.LogDebug("Test: CreateOrder_InvalidCustomerId_ReturnsNotFound");
            _logger.LogDebug($"Request URL: {_client.BaseAddress}orders/");

            // Act
            HttpResponseMessage response = await _client.PostAsJsonAsync("/orders", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            string content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"Response Status Code: {response.StatusCode}");
            _logger.LogDebug($"Response Content: {content}");
            content.Should().Contain("Customer with ID '999' not found.");
        }
    }
}