using OrderDemo.Application.DataTransferObjects.Orders;
using OrderDemo.Application.Common.Results;
using OneOf;

namespace OrderDemo.Application.Interfaces.Services {
    /// <summary>
    /// Defines the contract for managing order-related operations.
    /// </summary>
    public interface IOrderService {
        /// <summary>
        /// Creates a new order based on the provided request.
        /// </summary>
        /// <param name="request">The data transfer object containing order creation details.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="OneOf{T0, T1, T2, T3}"/> containing:
        /// <list type="bullet">
        /// <item><description><see cref="Success{OrderResponse}"/> if the order is created successfully.</description></item>
        /// <item><description><see cref="ValidationFailure"/> if the input request is invalid.</description></item>
        /// <item><description><see cref="NotFound"/> if the associated customer is not found.</description></item>
        /// <item><description><see cref="UnexpectedError"/> for any unforeseen issues during creation.</description></item>
        /// </list>
        /// </returns>
        Task<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an order by its unique identifier.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="OneOf{T0, T1, T2}"/> containing:
        /// <list type="bullet">
        /// <item><description><see cref="Success{OrderResponse}"/> if the order is found.</description></item>
        /// <item><description><see cref="NotFound"/> if the order with the specified ID is not found.</description></item>
        /// <item><description><see cref="UnexpectedError"/> for any unforeseen issues during retrieval.</description></item>
        /// </list>
        /// </returns>
        Task<OneOf<Success<OrderResponse>, NotFound, UnexpectedError>> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of all orders.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="OneOf{T0, T1}"/> containing:
        /// <list type="bullet">
        /// <item><description><see cref="Success{IEnumerable{OrderResponse}}"/> containing a list of all orders (can be empty).</description></item>
        /// <item><description><see cref="UnexpectedError"/> for any unforeseen issues during retrieval.</description></item>
        /// </list>
        /// </returns>
        Task<OneOf<Success<IEnumerable<OrderResponse>>, UnexpectedError>> GetAllOrdersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the status of a specific order.
        /// </summary>
        /// <param name="request">The data transfer object containing the order ID and the new status.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="OneOf{T0, T1, T2, T3}"/> containing:
        /// <list type="bullet">
        /// <item><description><see cref="Success{OrderResponse}"/> with the updated order details if the status update was successful.</description></item>
        /// <item><description><see cref="ValidationFailure"/> if the input request or status transition is invalid.</description></item>
        /// <item><description><see cref="NotFound"/> if the order with the specified ID is not found.</description></item>
        /// <item><description><see cref="UnexpectedError"/> for any unforeseen issues during update.</description></item>
        /// </list>
        /// </returns>
        Task<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>> UpdateOrderStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    }
}