using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MockECommerce.BusinessLayer.Exceptions;
using MockECommerce.BusinessLayer.Services;
using MockECommerce.DAL.Abstract;
using MockECommerce.DAL.Data;
using MockECommerce.DAL.Entities;
using MockECommerce.DtoLayer.OrderDtos;

namespace MockECommerce.BusinessLayer.Managers;

public class OrderManager : IOrderService
{
    private readonly IOrderDal _orderDal;
    private readonly IProductDal _productDal;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public OrderManager(IOrderDal orderDal, IProductDal productDal, IMapper mapper, AppDbContext context)
    {
        _orderDal = orderDal;
        _productDal = productDal;
        _mapper = mapper;
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        var product = await _productDal.GetByIdAsync(createOrderDto.ProductId);
        if (product == null)
        {
            throw new NotFoundException("Product not found.", "PRODUCT_NOT_FOUND");
        }

        var order = _mapper.Map<Order>(createOrderDto);
        order.Id = Guid.NewGuid();
        order.OrderDate = DateTime.UtcNow;

        await _orderDal.CreateAsync(order);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderDal.GetOrderWithProductDetailsByIdAsync(orderId);
        if (order == null)
        {
            throw new NotFoundException("Order not found.", "ORDER_NOT_FOUND");
        }

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderDal.GetOrdersWithProductDetailsAsync();
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task DeleteOrderAsync(Guid orderId)
    {
        var order = await _orderDal.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new NotFoundException("Order not found.", "ORDER_NOT_FOUND");
        }

        await _orderDal.DeleteAsync(order);
    }

    public async Task<List<OrderDto>> GetOrdersByCustomerIdAsync(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new BusinessException("Invalid customer ID.", "INVALID_CUSTOMER_ID");
        }

        var orders = await _orderDal.GetByCustomerIdAsync(customerId);
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<List<OrderDto>> GetOrdersBySellerIdAsync(Guid sellerId)
    {
        if (sellerId == Guid.Empty)
        {
            throw new BusinessException("Invalid seller ID.", "INVALID_SELLER_ID");
        }

        var orders = await _orderDal.GetOrdersBySellerIdAsync(sellerId);
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(UpdateOrderDto dto)
    {
        if (dto == null)
        {
            throw new BusinessException("Update order data is required.", "INVALID_UPDATE_DATA");
        }

        if (dto.Id == Guid.Empty)
        {
            throw new BusinessException("Invalid order ID.", "INVALID_ORDER_ID");
        }

        if (string.IsNullOrWhiteSpace(dto.Status))
        {
            throw new BusinessException("Order status is required.", "INVALID_ORDER_STATUS");
        }

        var order = await _orderDal.GetByIdAsync(dto.Id);
        if (order == null)
        {
            throw new NotFoundException("Order not found.", "ORDER_NOT_FOUND");
        }

        // Update the order status
        order.Status = dto.Status.Trim();
        
        await _orderDal.UpdateAsync(order);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<List<OrderDto>> GetOrdersBySellerUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new BusinessException("Invalid user ID.", "INVALID_USER_ID");
        }

        // Get SellerProfile from userId
        var sellerProfile = await _context.SellerProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId);
        if (sellerProfile == null)
        {
            throw new BusinessException("Seller profile not found.", "SELLER_PROFILE_NOT_FOUND");
        }

        var orders = await _orderDal.GetOrdersBySellerIdAsync(sellerProfile.Id);
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<bool> SellerHasAccessToSellerIdAsync(Guid userId, Guid sellerId)
    {
        if (userId == Guid.Empty || sellerId == Guid.Empty)
            return false;

        var sellerProfile = await _context.SellerProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId);
        
        return sellerProfile?.Id == sellerId;
    }
}
