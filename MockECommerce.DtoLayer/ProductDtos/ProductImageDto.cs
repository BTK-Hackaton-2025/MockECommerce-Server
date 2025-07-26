namespace MockECommerce.DtoLayer.ProductDtos;

public class ProductImageDto
{
    public Guid Id { get; set; }
    
    public string ImageUrl { get; set; } = default!;
    
    public string? AltText { get; set; }
    
    public bool IsPrimary { get; set; }
    
    public int DisplayOrder { get; set; }
}
