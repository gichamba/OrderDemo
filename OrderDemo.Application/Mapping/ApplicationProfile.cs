using AutoMapper;
using OrderDemo.Application.DataTransferObjects.Orders;
using OrderDemo.Domain.Entities;


namespace OrderDemo.Application.Mapping {
    /// <summary>
    /// AutoMapper profile for mapping between domain entities and data transfer objects.
    /// </summary>
    public class ApplicationProfile : Profile {
        public ApplicationProfile() {
            // Configure mapping from CreateOrderRequest DTO to Order Entity.
            CreateMap<CreateOrderRequest, Order>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
                .ForMember(dest => dest.OrderStatus, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountAmount, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveredDate, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore());

            // Configure mapping from Order Entity to OrderResponse DTO.
            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
                .ForMember(dest => dest.CustomerSegment, opt => opt.MapFrom(src => src.Customer.CustomerSegment))
                .ForMember(dest => dest.FinalAmount, opt => opt.MapFrom(src => src.FinalAmount));
        }
    }
}