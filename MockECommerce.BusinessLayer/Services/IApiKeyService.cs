using MockECommerce.DtoLayer.ApiKeyDtos;

namespace MockECommerce.BusinessLayer.Services;

public interface IApiKeyService
{
    
    /// Generates a new API key for the seller

    Task<ApiKeyDto> GenerateApiKeyAsync(Guid sellerId, CreateApiKeyDto createApiKeyDto);
    
    
    /// Gets all API keys for a seller
    
    Task<List<ApiKeyDto>> GetSellerApiKeysAsync(Guid sellerId);
    
    
    /// Gets a specific API key by ID (only if it belongs to the seller)
    
    Task<ApiKeyDto?> GetApiKeyByIdAsync(Guid sellerId, Guid apiKeyId);
    
    
    /// Validates an API key and returns the seller information
    
    Task<Guid?> ValidateApiKeyAsync(string apiKey);
    
    
    /// Updates API key usage timestamp
    
    Task UpdateLastUsedAsync(string apiKey);
    
    
    /// Deactivates an API key
    
    Task<bool> DeactivateApiKeyAsync(Guid sellerId, Guid apiKeyId);
    
    
    /// Activates an API key
   
    Task<bool> ActivateApiKeyAsync(Guid sellerId, Guid apiKeyId);
    
    
    /// Deletes an API key permanently
    
    Task<bool> DeleteApiKeyAsync(Guid sellerId, Guid apiKeyId);
}
