using System.ComponentModel.DataAnnotations;

namespace MockECommerce.DtoLayer.ProductDtos;

public class UpdateProductImageDto
{
    public Guid? Id { get; set; } // Null for new images
    
    [Required(ErrorMessage = "Resim URL'si gereklidir")]
    [Url(ErrorMessage = "Geçerli bir URL giriniz")]
    public string ImageUrl { get; set; } = default!;
    
    [StringLength(200, ErrorMessage = "Alt text 200 karakterden fazla olamaz")]
    public string? AltText { get; set; }
    
    public bool IsPrimary { get; set; } = false;
    
    [Range(0, int.MaxValue, ErrorMessage = "Görüntüleme sırası negatif olamaz")]
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsDeleted { get; set; } = false; // For marking images to be deleted
}
