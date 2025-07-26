using MockECommerce.DAL.Entities;

namespace MockECommerce.DtoLayer.CategoryDtos;

public class CategoryListDto
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string CategoryName { get; set; } = default!;

    public bool IsActive { get; set; }

    public string SeoSlug { get; set; } = default!;

    public List<CategoryListDto>? Children { get; set; }

}