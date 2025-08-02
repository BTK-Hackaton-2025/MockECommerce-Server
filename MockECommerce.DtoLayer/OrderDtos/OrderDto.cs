namespace MockECommerce.DtoLayer.OrderDtos;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; }
    public DateTime OrderDate { get; set; }
}
