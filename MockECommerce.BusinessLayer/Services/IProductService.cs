using MockECommerce.DAL.Enums;
using MockECommerce.DtoLayer.ProductDtos;

namespace MockECommerce.BusinessLayer.Services;

public interface IProductService
{
    // CRUD Operations (using User ID)
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, Guid userId);
    Task<ProductDto> UpdateProductAsync(UpdateProductDto updateProductDto, Guid userId);
    Task DeleteProductAsync(Guid id, Guid userId);
    
    // External API CRUD Operations (using SellerProfile ID directly)
    Task<ProductDto> CreateProductBySellerIdAsync(CreateProductDto createProductDto, Guid sellerId);
    
    // Admin-only CRUD Operations
    Task<ProductDto> UpdateProductAsAdminAsync(UpdateProductDto updateProductDto);
    Task DeleteProductAsAdminAsync(Guid id);
    
    // External API CRUD Operations (with SellerProfile ID)
    Task<ProductDto> UpdateProductBySellerIdAsync(UpdateProductDto updateProductDto, Guid sellerId);
    Task DeleteProductBySellerIdAsync(Guid id, Guid sellerId);
    
    // Read Operations
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<ProductDto> GetProductByIdAsync(Guid id);
    Task<List<ProductDto>> GetProductsByCategoryIdAsync(Guid categoryId);
    Task<List<ProductDto>> GetProductsBySellerIdAsync(Guid sellerId);
    Task<List<ProductDto>> GetProductsByUserIdAsync(Guid userId);
    Task<List<ProductDto>> GetActiveProductsAsync();
    Task<List<ProductDto>> GetProductsByStatusAsync(ProductStatus status);
    
    // Search and Filter Operations
    Task<List<ProductDto>> SearchProductsAsync(string searchTerm);
    Task<List<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    
    // Admin Operations
    Task<ProductDto> UpdateProductStatusAsync(Guid id, ProductStatus status);
    Task<ProductDto> ToggleProductActiveStatusAsync(Guid id);
    
    // Seller Profile Operations
    Task<object> CreateSellerProfileAsync(Guid userId, string storeName);
    Task<object?> GetSellerProfileByUserIdAsync(Guid userId);
}
