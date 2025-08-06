namespace MockECommerce.DtoLayer.OrderDtos;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string ProductImageUrl { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime OrderDate { get; set; }
}
