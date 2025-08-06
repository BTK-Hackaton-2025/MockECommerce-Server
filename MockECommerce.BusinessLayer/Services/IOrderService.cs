using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MockECommerce.DtoLayer.OrderDtos;

namespace MockECommerce.BusinessLayer.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId);
    Task<List<OrderDto>> GetAllOrdersAsync();
    Task DeleteOrderAsync(Guid orderId);
    Task<List<OrderDto>> GetOrdersByCustomerIdAsync(Guid customerId);
    Task<List<OrderDto>> GetOrdersBySellerIdAsync(Guid sellerId);
    Task<List<OrderDto>> GetOrdersBySellerUserIdAsync(Guid userId);
    Task<bool> SellerHasAccessToSellerIdAsync(Guid userId, Guid sellerId);
    Task<OrderDto> UpdateOrderStatusAsync(UpdateOrderDto dto);
}
