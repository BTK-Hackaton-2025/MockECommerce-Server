using Microsoft.EntityFrameworkCore;
using MockECommerce.DAL.Abstract;
using MockECommerce.DAL.Data;
using MockECommerce.DAL.Entities;

namespace MockECommerce.DAL.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderDal
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersWithProductDetailsAsync()
    {
        return await _context.Orders
            .Include(o => o.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersBySellerIdAsync(Guid sellerId)
    {
        return await _context.Orders
            .Include(o => o.Product)
            .Where(o => o.Product.SellerId == sellerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithProductDetailsByIdAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Order>> GetByStatusAsync(string status)
    {
        return await _context.Orders
            .Include(o => o.Product)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
}
