using MockECommerce.DAL.Entities;

namespace MockECommerce.DtoLayer.CategoryDtos;

public class CategoryDetailDto
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string CategoryName { get; set; } = default!;

    public string? ParentName { get; set; }

    public bool IsActive { get; set; }

    public string SeoSlug { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<CategoryListDto>? Children { get; set; }
}