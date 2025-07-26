using MockECommerce.DAL.Enums;
using MockECommerce.DtoLayer.ProductDtos;

namespace MockECommerce.BusinessLayer.Services;

public interface IProductService
{
    // CRUD Operations
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, Guid sellerId);
    Task<ProductDto> UpdateProductAsync(UpdateProductDto updateProductDto, Guid sellerId);
    Task DeleteProductAsync(Guid id, Guid sellerId);
    
    // Read Operations
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<ProductDto> GetProductByIdAsync(Guid id);
    Task<List<ProductDto>> GetProductsByCategoryIdAsync(Guid categoryId);
    Task<List<ProductDto>> GetProductsBySellerIdAsync(Guid sellerId);
    Task<List<ProductDto>> GetActiveProductsAsync();
    Task<List<ProductDto>> GetProductsByStatusAsync(ProductStatus status);
    
    // Search and Filter Operations
    Task<List<ProductDto>> SearchProductsAsync(string searchTerm);
    Task<List<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    
    // Admin Operations
    Task<ProductDto> UpdateProductStatusAsync(Guid id, ProductStatus status);
    Task<ProductDto> ToggleProductActiveStatusAsync(Guid id);
}
