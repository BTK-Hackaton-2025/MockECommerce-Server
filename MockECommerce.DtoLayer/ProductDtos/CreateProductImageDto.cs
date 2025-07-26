using System.ComponentModel.DataAnnotations;

namespace MockECommerce.DtoLayer.ProductDtos;

public class CreateProductImageDto
{
    [Required(ErrorMessage = "Resim URL'si gereklidir")]
    [Url(ErrorMessage = "Geçerli bir URL giriniz")]
    public string ImageUrl { get; set; } = default!;
    
    [StringLength(200, ErrorMessage = "Alt text 200 karakterden fazla olamaz")]
    public string? AltText { get; set; }
    
    public bool IsPrimary { get; set; } = false;
    
    [Range(0, int.MaxValue, ErrorMessage = "Görüntüleme sırası negatif olamaz")]
    public int DisplayOrder { get; set; } = 0;
}
