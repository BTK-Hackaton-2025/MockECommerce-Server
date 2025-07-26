namespace MockECommerce.DAL.Entities;

public class ProductImage
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string ImageUrl { get; set; } = default!;

    public string? AltText { get; set; }

    public bool IsPrimary { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public Product Product { get; set; } = default!;
}