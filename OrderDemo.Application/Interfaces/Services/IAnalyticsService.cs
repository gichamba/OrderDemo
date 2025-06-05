using OrderDemo.Application.DataTransferObjects.Analytics;
using OrderDemo.Application.Common.Results;
using OneOf;

namespace OrderDemo.Application.Interfaces.Services {
    /// <summary>
    /// Defines the contract for retrieving order analytics.
    /// </summary>
    public interface IAnalyticsService {
        /// <summary>
        /// Retrieves aggregated analytical data about orders.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="OneOf{T0, T1}"/> containing:
        /// <list type="bullet">
        /// <item><description><see cref="Success{OrderAnalyticsDto}"/> containing various order statistics.</description></item>
        /// <item><description><see cref="UnexpectedError"/> for any unforeseen issues during retrieval.</description></item>
        /// </list>
        /// </returns>
        Task<OneOf<Success<OrderAnalyticsDto>, UnexpectedError>> GetOrderAnalyticsAsync(CancellationToken cancellationToken = default);
    }
}