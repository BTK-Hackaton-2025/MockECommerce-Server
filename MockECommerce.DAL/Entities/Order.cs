namespace MockECommerce.DAL.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; }
    public DateTime OrderDate { get; set; }
    
    public Order()
    {
        Status = "Pending";
        OrderDate = DateTime.UtcNow;
    }
}
