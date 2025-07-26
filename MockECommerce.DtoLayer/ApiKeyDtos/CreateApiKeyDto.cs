using System.ComponentModel.DataAnnotations;

namespace MockECommerce.DtoLayer.ApiKeyDtos;

public class CreateApiKeyDto
{
    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "API key name must be between 3-100 characters")]
    public string Name { get; set; } = default!;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    [Range(1, 1000, ErrorMessage = "Requests per minute must be between 1-1000")]
    public int RequestsPerMinute { get; set; } = 60;
    
    [Range(1, 100000, ErrorMessage = "Requests per day must be between 1-100000")]
    public int RequestsPerDay { get; set; } = 10000;
    
    public DateTime? ExpiresAt { get; set; }
}
