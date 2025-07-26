using MockECommerce.DAL.Entities;
using MockECommerce.DAL.Enums;

namespace MockECommerce.DAL.Abstract;

public interface IProductDal : IGenericDal<Product>
{
    Task<List<Product>> GetAllWithDetailsAsync();
    
    Task<Product?> GetByIdWithDetailsAsync(Guid id);
    
    Task<List<Product>> GetByCategoryIdAsync(Guid categoryId);
    
    Task<List<Product>> GetBySellerIdAsync(Guid sellerId);
    
    Task<List<Product>> GetByStatusAsync(ProductStatus status);
    
    Task<List<Product>> GetActiveProductsAsync();
    
    Task<List<Product>> SearchProductsAsync(string searchTerm);
    
    Task<List<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
}
