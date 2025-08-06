using System.ComponentModel.DataAnnotations;

namespace MockECommerce.DtoLayer.OrderDtos;

public class CreateOrderDto
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }
    
    public Guid CustomerId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;
    
    public string Notes { get; set; } = string.Empty;
}
