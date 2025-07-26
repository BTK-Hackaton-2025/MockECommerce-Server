namespace MockECommerce.DAL.Entities;

public class Category
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }

    public string CategoryName { get; set; }

    public bool IsActive { get; set; }

    public string SeoSlug { get; set; } = default!;
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; }
}