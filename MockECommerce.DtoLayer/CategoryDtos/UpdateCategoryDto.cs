using System.ComponentModel.DataAnnotations;

namespace MockECommerce.DtoLayer.CategoryDtos;

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "ID gereklidir")]
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    [Required(ErrorMessage = "Kategori adı gereklidir")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı 2-100 karakter arasında olmalıdır")]
    public string CategoryName { get; set; } = default!;

    public string? ParentName { get; set; }

    public bool IsActive { get; set; }

    [Required(ErrorMessage = "SEO slug gereklidir")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "SEO slug 2-150 karakter arasında olmalıdır")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "SEO slug sadece küçük harf, rakam ve tire içerebilir")]
    public string SeoSlug { get; set; } = default!;
}
