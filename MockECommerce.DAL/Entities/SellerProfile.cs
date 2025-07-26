namespace MockECommerce.DAL.Entities;

public class SellerProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string StoreName { get; set; } = default!;

    public string? LogoUrl { get; set; }

    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = default!;

    public ICollection<Product> Products { get; set; } = new List<Product>();

    public ICollection<MarketplaceCredential> Credentials { get; set; } = new List<MarketplaceCredential>();
    
    public ICollection<SellerApiKey> ApiKeys { get; set; } = new List<SellerApiKey>();
}