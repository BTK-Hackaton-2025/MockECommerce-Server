namespace MockECommerce.DAL.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public AppUser Customer { get; set; } = default!;
    public int Quantity { get; set; } = 1;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime OrderDate { get; set; }
    
    public Order()
    {
        Status = "Pending";
        OrderDate = DateTime.UtcNow;
    }
}
