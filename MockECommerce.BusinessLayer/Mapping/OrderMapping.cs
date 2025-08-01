using AutoMapper;
using MockECommerce.DAL.Entities;
using MockECommerce.DtoLayer.OrderDtos;

namespace MockECommerce.BusinessLayer.Mapping;

public class OrderMapping : Profile
{
    public OrderMapping()
    {
        // Order Entity to OrderDto
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Title));

        // CreateOrderDto to Order Entity
        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
            .ForMember(dest => dest.Product, opt => opt.Ignore());

        // UpdateOrderDto to Order Entity
        CreateMap<UpdateOrderDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.OrderDate, opt => opt.Ignore());
    }
}
