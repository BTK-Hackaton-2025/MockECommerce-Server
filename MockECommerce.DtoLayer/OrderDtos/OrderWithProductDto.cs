using System.ComponentModel.DataAnnotations;

namespace MockECommerce.DtoLayer.OrderDtos;

public class OrderWithProductDto
{
    [Required(ErrorMessage = "Sipariş ID gereklidir")]
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Ürün ID gereklidir")]
    public Guid ProductId { get; set; }
    
    [Required(ErrorMessage = "Ürün adı gereklidir")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Ürün adı 2-200 karakter arasında olmalıdır")]
    public string ProductName { get; set; } = default!;
    
    [StringLength(2000, ErrorMessage = "Ürün açıklaması en fazla 2000 karakter olabilir")]
    public string? ProductDescription { get; set; }
    
    [Required(ErrorMessage = "Ürün fiyatı gereklidir")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Ürün fiyatı 0'dan büyük olmalıdır")]
    public decimal ProductPrice { get; set; }
    
    [Required(ErrorMessage = "Kategori ID gereklidir")]
    public Guid CategoryId { get; set; }
    
    [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
    public string? CategoryName { get; set; }
    
    [Required(ErrorMessage = "Müşteri ID gereklidir")]
    public Guid CustomerId { get; set; }
    
    [StringLength(100, ErrorMessage = "Müşteri adı en fazla 100 karakter olabilir")]
    public string? CustomerName { get; set; }
    
    [Required(ErrorMessage = "Sipariş durumu gereklidir")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Sipariş durumu 3-50 karakter arasında olmalıdır")]
    public string Status { get; set; } = default!;
    
    [Required(ErrorMessage = "Sipariş tarihi gereklidir")]
    public DateTime OrderDate { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}
