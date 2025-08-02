using MockECommerce.DAL.Entities;

namespace MockECommerce.DAL.Abstract;

public interface IOrderDal : IGenericDal<Order>
{
    Task<List<Order>> GetByCustomerIdAsync(Guid customerId);
    
    Task<List<Order>> GetOrdersWithProductDetailsAsync();
    
    Task<List<Order>> GetByStatusAsync(string status);
    
    Task<List<Order>> GetOrdersBySellerIdAsync(Guid sellerId);
    
    Task<Order?> GetOrderWithProductDetailsByIdAsync(Guid orderId);
}
