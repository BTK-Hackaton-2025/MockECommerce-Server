using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MockECommerce.BusinessLayer.Services;
using MockECommerce.DtoLayer.OrderDtos;

namespace MockECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Get all orders (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(new { success = true, data = orders });
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Invalid order ID" });

        var order = await _orderService.GetOrderByIdAsync(id);
        return Ok(new { success = true, data = order });
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

        var order = await _orderService.CreateOrderAsync(createOrderDto);

        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id },
            new { success = true, data = order });
    }

    /// <summary>
    /// Get orders by seller ID
    /// </summary>
    [HttpGet("seller/{sellerId}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetOrdersBySellerId(Guid sellerId)
    {
        if (sellerId == Guid.Empty)
            return BadRequest(new { success = false, message = "Invalid seller ID" });

        var orders = await _orderService.GetOrdersBySellerIdAsync(sellerId);
        return Ok(new { success = true, data = orders });
    }

    /// <summary>
    /// Get a specific order by ID (Seller)
    /// </summary>
    [HttpGet("seller/order/{orderId}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetOrderByIdForSeller(Guid orderId)
    {
        if (orderId == Guid.Empty)
            return BadRequest(new { success = false, message = "Invalid order ID" });

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.ProductId == Guid.Empty)
            return NotFound(new { success = false, message = "Order not found" });

        return Ok(new { success = true, data = order });
    }

    /// <summary>
    /// Delete an order
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Invalid order ID" });

        await _orderService.DeleteOrderAsync(id);
        return Ok(new { success = true, message = "Order deleted successfully" });
    }

    /// <summary>
    /// Get orders by customer ID
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [Authorize]
    public async Task<IActionResult> GetOrdersByCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
            return BadRequest(new { success = false, message = "Invalid customer ID" });

        var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
        return Ok(new { success = true, data = orders });
    }

    /// <summary>
    /// Update order status (Admin or Seller only)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderDto updateOrderDto)
    {
        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Invalid order ID" });

        if (id != updateOrderDto.Id)
            return BadRequest(new { success = false, message = "ID mismatch" });

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

        var updatedOrder = await _orderService.UpdateOrderStatusAsync(updateOrderDto);
        return Ok(new { success = true, data = updatedOrder });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("uuid") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return Guid.Empty;
    }
}
