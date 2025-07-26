using System.ComponentModel.DataAnnotations;
using MockECommerce.DAL.Enums;

namespace MockECommerce.DtoLayer.ProductDtos;

public class CreateProductDto
{
    [Required(ErrorMessage = "Kategori ID gereklidir")]
    public Guid CategoryId { get; set; }
    
    [Required(ErrorMessage = "Ürün başlığı gereklidir")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Ürün başlığı 2-200 karakter arasında olmalıdır")]
    public string Title { get; set; } = default!;
    
    [Required(ErrorMessage = "Ürün açıklaması gereklidir")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Ürün açıklaması 10-2000 karakter arasında olmalıdır")]
    public string Description { get; set; } = default!;
    
    [Required(ErrorMessage = "Fiyat gereklidir")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
    public decimal Price { get; set; }
    
    [Required(ErrorMessage = "Stok adedi gereklidir")]
    [Range(0, int.MaxValue, ErrorMessage = "Stok adedi negatif olamaz")]
    public int Stock { get; set; }
    
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    
    public List<CreateProductImageDto> Images { get; set; } = new();
}
