using MockECommerce.DAL.Enums;

namespace MockECommerce.DAL.Entities;

public class Product
{
    public Guid Id { get; set; }

    public Guid SellerId { get; set; }

    public Guid CategoryId { get; set; }

    public Category Category { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    public SellerProfile Seller { get; set; } = default!;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

}