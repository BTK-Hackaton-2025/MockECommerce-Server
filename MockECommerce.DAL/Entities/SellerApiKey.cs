namespace MockECommerce.DAL.Entities;

public class SellerApiKey
{
    public Guid Id { get; set; }
    
    public Guid SellerId { get; set; }
    
    public string ApiKey { get; set; } = default!;
    
    public string Name { get; set; } = default!; // Friendly name for the API key
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastUsedAt { get; set; }
    
    public DateTime? ExpiresAt { get; set; } // Optional expiration
    
    public string? Description { get; set; }
    
    // Rate limiting fields
    public int RequestsPerMinute { get; set; } = 60; // Default rate limit
    
    public int RequestsPerDay { get; set; } = 10000; // Daily request limit
    
    // Navigation properties
    public SellerProfile Seller { get; set; } = default!;
}
