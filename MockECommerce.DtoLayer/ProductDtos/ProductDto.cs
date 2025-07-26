using MockECommerce.DAL.Enums;

namespace MockECommerce.DtoLayer.ProductDtos;

public class ProductDto
{
    public Guid Id { get; set; }
    
    public Guid SellerId { get; set; }
    
    public string SellerName { get; set; } = default!;
    
    public Guid CategoryId { get; set; }
    
    public string CategoryName { get; set; } = default!;
    
    public string Title { get; set; } = default!;
    
    public string Description { get; set; } = default!;
    
    public decimal Price { get; set; }
    
    public int Stock { get; set; }
    
    public ProductStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public bool IsActive { get; set; }
    
    public List<ProductImageDto> Images { get; set; } = new();
}
