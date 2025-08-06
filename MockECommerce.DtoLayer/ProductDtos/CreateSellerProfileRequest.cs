using System.ComponentModel.DataAnnotations;

namespace MockECommerce.DtoLayer.ProductDtos;

public class CreateSellerProfileRequest
{
    [Required(ErrorMessage = "Mağaza adı gereklidir")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Mağaza adı 2-200 karakter arasında olmalıdır")]
    public string StoreName { get; set; } = default!;
}
