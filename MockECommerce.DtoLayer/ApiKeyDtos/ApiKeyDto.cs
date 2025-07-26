namespace MockECommerce.DtoLayer.ApiKeyDtos;

public class ApiKeyDto
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = default!;
    
    public string ApiKey { get; set; } = default!;
    
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastUsedAt { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    public string? Description { get; set; }
    
    public int RequestsPerMinute { get; set; }
    
    public int RequestsPerDay { get; set; }
}
