namespace MockECommerce.DtoLayer.OrderDtos;

public class CreateOrderDto
{
    public Guid ProductId { get; set; }
    public Guid CustomerId { get; set; }
}
