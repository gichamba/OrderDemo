using Microsoft.AspNetCore.Mvc;
using OrderDemo.Application.Common.Results;
using OrderDemo.Application.DataTransferObjects.Analytics;
using OrderDemo.Application.DataTransferObjects.Orders;
using OrderDemo.Application.Interfaces.Services;
using OneOf;

namespace OrderDemo.WebApi.Extensions {
    /// <summary>
    /// Provides extension methods for mapping Minimal API endpoints.
    /// </summary>
    public static class EndpointExtensions {
        public static WebApplication MapEndPoints(this WebApplication app) {
            // Group API endpoints by logical resource.
            RouteGroupBuilder ordersApi = app.MapGroup("/orders")
                                                .WithTags("Orders")
                                                .WithOpenApi();

            RouteGroupBuilder analyticsApi = app.MapGroup("/analytics")
                                                    .WithTags("Analytics")
                                                    .WithOpenApi();

            ordersApi.MapPost("/", CreateOrder);
            ordersApi.MapGet("/{id:int}", GetOrderById);
            ordersApi.MapGet("/", GetAllOrders);
            ordersApi.MapPut("/status", UpdateOrderStatus);

            analyticsApi.MapGet("/orders", GetOrderAnalytics);

            return app;
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="request">The order creation request data.</param>
        /// <param name="orderService">The order service.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created order details or an error.</returns>
        private static async Task<IResult> CreateOrder(
            CreateOrderRequest request,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) {
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await orderService.CreateOrderAsync(request, cancellationToken);

            return result.Match(
                success => Results.Created($"/orders/{success.Data.Id}", success.Data), 
                validationFailure => Results.ValidationProblem(validationFailure.Errors.ToDictionary(
                        error => string.Join(", ", error.MemberNames), error => new[] { error.Message })), 
                notFound => Results.NotFound(notFound.Message), 
                unexpectedError => Results.Problem(unexpectedError.Message, statusCode: 500)
            );
        }

        /// <summary>
        /// Retrieves an order by its ID.
        /// </summary>
        /// <param name="id">The ID of the order to retrieve.</param>
        /// <param name="orderService">The order service.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The order details or an error.</returns>
        private static async Task<IResult> GetOrderById(
            int id,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) {
            OneOf<Success<OrderResponse>, NotFound, UnexpectedError> result = await orderService.GetOrderByIdAsync(id, cancellationToken);

            return result.Match(
                success => Results.Ok(success.Data),
                notFound => Results.NotFound(notFound.Message),
                unexpectedError => Results.Problem(unexpectedError.Message, statusCode: 500)
            );
        }

        /// <summary>
        /// Retrieves all orders.
        /// </summary>
        /// <param name="orderService">The order service.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of all orders or an error.</returns>
        private static async Task<IResult> GetAllOrders(
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) {
            OneOf<Success<IEnumerable<OrderResponse>>, UnexpectedError> result = await orderService.GetAllOrdersAsync(cancellationToken);

            return result.Match(
                success => Results.Ok(success.Data),
                unexpectedError => Results.Problem(unexpectedError.Message, statusCode: 500)
            );
        }

        /// <summary>
        /// Updates the status of an existing order.
        /// </summary>
        /// <param name="request">The update order status request data.</param>
        /// <param name="orderService">The order service.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated order details or an error.</returns>
        private static async Task<IResult> UpdateOrderStatus(
            UpdateOrderStatusRequest request,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) {
            OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError> result = await orderService.UpdateOrderStatusAsync(request, cancellationToken);

            return result.Match(
                success => Results.Ok(success.Data),
                validationFailure => Results.ValidationProblem(validationFailure.Errors.ToDictionary(error => string.Join(", ", error.MemberNames), error => new[] { error.Message })),
                notFound => Results.NotFound(notFound.Message),
                unexpectedError => Results.Problem(unexpectedError.Message, statusCode: 500)
            );
        }

        /// <summary>
        /// Retrieves aggregated order analytics.
        /// </summary>
        /// <param name="analyticsService">The analytics service.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Order analytics data or an error.</returns>
        private static async Task<IResult> GetOrderAnalytics([FromServices] IAnalyticsService analyticsService, CancellationToken cancellationToken) {
            OneOf<Success<OrderAnalyticsDto>, UnexpectedError> result = await analyticsService.GetOrderAnalyticsAsync(cancellationToken);
            return result.Match(success => Results.Ok(success.Data),
                unexpectedError => Results.Problem(unexpectedError.Message, statusCode: 500)
            );
        }
    }
}