using Microsoft.EntityFrameworkCore;
using OrderDemo.Application.Common.Results;
using OrderDemo.Application.DataTransferObjects.Analytics;
using OrderDemo.Application.Interfaces.Contexts;
using OrderDemo.Application.Interfaces.Services;
using OrderDemo.Domain.Enums;
using OneOf;
using OrderDemo.Domain.Entities;

namespace OrderDemo.Application.Services {
    /// <summary>
    /// Provides services for retrieving analytical data about orders.
    /// </summary>
    public class AnalyticsService(IOrderDemoContext context) : IAnalyticsService {
        private readonly IOrderDemoContext _context = context ?? throw new ArgumentNullException(nameof(context));

        /// <inheritdoc />
        public async Task<OneOf<Success<OrderAnalyticsDto>, UnexpectedError>> GetOrderAnalyticsAsync(CancellationToken cancellationToken = default) {
            try {
                List<Order> allOrders = await _context.Orders.ToListAsync(cancellationToken);

                int totalOrders = allOrders.Count;
                decimal averageOrderValue = totalOrders > 0 ? allOrders.Average(o => o.FinalAmount) : 0m;

                List<Order> deliveredOrders = allOrders.Where(o => o is { OrderStatus: OrderStatus.Delivered, DeliveredDate: not null }).ToList();

                double averageFulfillmentTimeInHours = 0.0;
                if (deliveredOrders.Any()) {
                    averageFulfillmentTimeInHours = deliveredOrders.Average(o => (o.DeliveredDate!.Value - o.OrderDate).TotalHours);
                }

                int totalPendingOrders = allOrders.Count(o => o.OrderStatus == OrderStatus.Pending);
                int totalDeliveredOrders = deliveredOrders.Count;

                OrderAnalyticsDto analyticsDto = new() {
                    AverageOrderValue = averageOrderValue,
                    AverageFulfillmentTimeInHours = averageFulfillmentTimeInHours,
                    TotalOrders = totalOrders,
                    TotalPendingOrders = totalPendingOrders,
                    TotalDeliveredOrders = totalDeliveredOrders
                };

                return new Success<OrderAnalyticsDto>(analyticsDto);
            } catch (Exception ex) {
                return new UnexpectedError($"An unexpected error occurred while retrieving order analytics: {ex.Message}", ex);
            }
        }
    }
}