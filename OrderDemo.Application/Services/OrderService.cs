using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrderDemo.Application.Common.Results;
using OrderDemo.Application.DataTransferObjects.Orders;
using OrderDemo.Application.Interfaces.Contexts;
using OrderDemo.Application.Interfaces.Services;
using OrderDemo.Application.Utilities;
using OrderDemo.Domain.Entities;
using OrderDemo.Domain.Enums;
using OneOf;


namespace OrderDemo.Application.Services {
    /// <summary>
    /// Provides services for managing orders, including creation, retrieval, and status updates.
    /// </summary>
    public class OrderService(IOrderDemoContext context, IMapper mapper, IDiscountService discountService) : IOrderService {
        private readonly IOrderDemoContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly IDiscountService _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));

        /// <inheritdoc />
        public async Task<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default) {
            // Validate DTO
            OneOf<Success<CreateOrderRequest>, ValidationFailure> validationResult = ValidatorService.ValidateDto(request);

            if (validationResult.IsT1) {
                return validationResult.AsT1; // ValidationFailure
            }

            try {
                // Check if customer exists
                // OPTIMIZATION: Eagerly load the Customer along with their Orders to prevent N+1 queries
                Customer? customer = await _context.Customers.Include(c => c.Orders) // <--- OPTIMIZATION ALREADY HERE
                    .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

                if (customer == null) {
                    return new NotFound($"Customer with ID '{request.CustomerId}' not found.");
                }

                // Map DTO to Entity and initialize order details
                Order? order = _mapper.Map<Order>(request);
                order.OrderDate = DateTime.UtcNow;
                order.OrderStatus = OrderStatus.Pending;
                order.DiscountAmount = 0.00m;

                // Apply Discount, which updates order.DiscountAmount internally
                order.DiscountAmount = _discountService.ApplyDiscount(order, customer);

                // Add to context and save changes
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(cancellationToken);

                // Map back to Response DTO, ensuring customer details are populated
                OrderResponse? orderResponse = _mapper.Map<OrderResponse>(order);
                orderResponse.CustomerName = customer.Name;
                orderResponse.CustomerSegment = customer.CustomerSegment;

                return new Success<OrderResponse>(orderResponse);
            } catch (DbUpdateException dbEx) {
                return new UnexpectedError($"A database error occurred while creating the order: {dbEx.Message}", dbEx);
            } catch (Exception ex) {
                return new UnexpectedError($"An unexpected error occurred while creating the order: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<OneOf<Success<OrderResponse>, NotFound, UnexpectedError>> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default) {
            try {
                Order? order = await _context.Orders
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

                if (order == null) {
                    return new NotFound($"Order with ID '{orderId}' not found.");
                }

                OrderResponse? orderResponse = _mapper.Map<OrderResponse>(order);
                orderResponse.CustomerName = order.Customer.Name;
                orderResponse.CustomerSegment = order.Customer.CustomerSegment;

                return new Success<OrderResponse>(orderResponse);
            } catch (DbUpdateException dbEx) {
                return new UnexpectedError($"A database error occurred while retrieving order {orderId}: {dbEx.Message}", dbEx);
            } catch (Exception ex) {
                return new UnexpectedError($"An unexpected error occurred while retrieving order {orderId}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<OneOf<Success<IEnumerable<OrderResponse>>, UnexpectedError>> GetAllOrdersAsync(CancellationToken cancellationToken = default) {
            try {
                List<Order> orders = await _context.Orders
                    .Include(o => o.Customer)
                    .ToListAsync(cancellationToken);

                List<OrderResponse> orderResponses = orders.Select(o => {
                    OrderResponse? response = _mapper.Map<OrderResponse>(o);
                    response.CustomerName = o.Customer.Name;
                    response.CustomerSegment = o.Customer.CustomerSegment;
                    return response;
                }).ToList();

                return new Success<IEnumerable<OrderResponse>>(orderResponses);
            } catch (DbUpdateException dbEx) {
                return new UnexpectedError($"A database error occurred while retrieving all orders: {dbEx.Message}", dbEx);
            } catch (Exception ex) {
                return new UnexpectedError($"An unexpected error occurred while retrieving all orders: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<OneOf<Success<OrderResponse>, ValidationFailure, NotFound, UnexpectedError>> UpdateOrderStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default) {
            // Validate DTO
            OneOf<Success<UpdateOrderStatusRequest>, ValidationFailure> validationResult = ValidatorService.ValidateDto(request);
            if (validationResult.IsT1) {
                return validationResult.AsT1; // ValidationFailure
            }

            try {
                Order? order = await _context.Orders
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

                if (order == null) {
                    return new NotFound($"Order with ID '{request.OrderId}' not found.");
                }

                // Implement state transition logic
                OrderStatus currentStatus = order.OrderStatus;
                OrderStatus newStatus = request.NewStatus;

                if (currentStatus == newStatus) {
                    return new ValidationFailure(new List<ValidationError> {
                        new($"Order is already in '{newStatus}' status.", [nameof(request.NewStatus)])
                    }.AsReadOnly());
                }

                // Basic valid transitions
                if (currentStatus == OrderStatus.Pending && newStatus is OrderStatus.Processing or OrderStatus.Cancelled) {
                    order.OrderStatus = newStatus;
                } else if (currentStatus == OrderStatus.Processing && newStatus is OrderStatus.Shipped or OrderStatus.Cancelled) {
                    order.OrderStatus = newStatus;
                } else if (currentStatus == OrderStatus.Shipped && newStatus == OrderStatus.Delivered) {
                    order.OrderStatus = newStatus;
                    order.DeliveredDate = DateTime.UtcNow; // Set delivered date
                } else if (newStatus == OrderStatus.Cancelled && currentStatus is OrderStatus.Pending or OrderStatus.Processing) {
                    order.OrderStatus = newStatus;
                } else {
                    return new ValidationFailure(new List<ValidationError>
                    {
                        new($"Invalid status transition from '{currentStatus}' to '{newStatus}'.", [nameof(request.NewStatus)])
                    }.AsReadOnly());
                }

                // Save changes
                await _context.SaveChangesAsync(cancellationToken);

                // Return updated OrderResponse
                OrderResponse? orderResponse = _mapper.Map<OrderResponse>(order);
                orderResponse.CustomerName = order.Customer.Name;
                orderResponse.CustomerSegment = order.Customer.CustomerSegment;

                return new Success<OrderResponse>(orderResponse);
            } catch (DbUpdateConcurrencyException dbConcEx) {
                return new UnexpectedError($"A concurrency error occurred while updating the order status: {dbConcEx.Message}", dbConcEx);
            } catch (DbUpdateException dbEx) {
                return new UnexpectedError($"A database error occurred while updating the order status: {dbEx.Message}", dbEx);
            } catch (Exception ex) {
                return new UnexpectedError($"An unexpected error occurred while updating the order status: {ex.Message}", ex);
            }
        }
    }
}