using MockECommerce.DAL.Enums;

namespace MockECommerce.DAL.Entities;

public class MarketplaceCredential
{
    public Guid Id { get; set; }

    public Guid SellerId { get; set; }

    public MarketplacePlatform Platform { get; set; }

    public string ApiKey { get; set; } = default!;

    public string Secret { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SellerProfile Seller { get; set; } = default!;
}