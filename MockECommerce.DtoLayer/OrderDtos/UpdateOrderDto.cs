using System.ComponentModel.DataAnnotations;

namespace MockECommerce.DtoLayer.OrderDtos;

public class UpdateOrderDto
{
    [Required(ErrorMessage = "Sipariş ID gereklidir")]
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Sipariş durumu gereklidir")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Sipariş durumu 3-50 karakter arasında olmalıdır")]
    public string Status { get; set; } = default!;
}
