using Microsoft.EntityFrameworkCore;
using MockECommerce.BusinessLayer.Services;
using MockECommerce.BusinessLayer.Utils;
using MockECommerce.DAL.Data;
using MockECommerce.DAL.Entities;
using MockECommerce.DtoLayer.ApiKeyDtos;

namespace MockECommerce.BusinessLayer.Managers;

public class ApiKeyManager : IApiKeyService
{
    private readonly AppDbContext _context;

    public ApiKeyManager(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiKeyDto> GenerateApiKeyAsync(Guid sellerId, CreateApiKeyDto createApiKeyDto)
    {
        // Verify seller exists
        var seller = await _context.SellerProfiles.FindAsync(sellerId);
        if (seller == null)
            throw new ArgumentException("Seller not found", nameof(sellerId));

        // Check if seller already has an API key with the same name
        var existingKey = await _context.SellerApiKeys
            .FirstOrDefaultAsync(sak => sak.SellerId == sellerId && sak.Name == createApiKeyDto.Name);
        
        if (existingKey != null)
            throw new InvalidOperationException($"API key with name '{createApiKeyDto.Name}' already exists");

        // Generate unique API key
        string apiKey;
        bool isUnique = false;
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            apiKey = ApiKeyGenerator.GenerateApiKey();
            var existing = await _context.SellerApiKeys
                .FirstOrDefaultAsync(sak => sak.ApiKey == apiKey);
            
            isUnique = existing == null;
            attempts++;
            
            if (attempts >= maxAttempts)
                throw new InvalidOperationException("Unable to generate unique API key after multiple attempts");
                
        } while (!isUnique);

        // Create new API key entity
        var sellerApiKey = new SellerApiKey
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            ApiKey = apiKey,
            Name = createApiKeyDto.Name,
            Description = createApiKeyDto.Description,
            RequestsPerMinute = createApiKeyDto.RequestsPerMinute,
            RequestsPerDay = createApiKeyDto.RequestsPerDay,
            ExpiresAt = createApiKeyDto.ExpiresAt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.SellerApiKeys.AddAsync(sellerApiKey);
        await _context.SaveChangesAsync();

        return new ApiKeyDto
        {
            Id = sellerApiKey.Id,
            Name = sellerApiKey.Name,
            ApiKey = sellerApiKey.ApiKey,
            IsActive = sellerApiKey.IsActive,
            CreatedAt = sellerApiKey.CreatedAt,
            LastUsedAt = sellerApiKey.LastUsedAt,
            ExpiresAt = sellerApiKey.ExpiresAt,
            Description = sellerApiKey.Description,
            RequestsPerMinute = sellerApiKey.RequestsPerMinute,
            RequestsPerDay = sellerApiKey.RequestsPerDay
        };
    }

    public async Task<List<ApiKeyDto>> GetSellerApiKeysAsync(Guid sellerId)
    {
        var apiKeys = await _context.SellerApiKeys
            .Where(sak => sak.SellerId == sellerId)
            .OrderByDescending(sak => sak.CreatedAt)
            .Select(sak => new ApiKeyDto
            {
                Id = sak.Id,
                Name = sak.Name,
                ApiKey = sak.ApiKey,
                IsActive = sak.IsActive,
                CreatedAt = sak.CreatedAt,
                LastUsedAt = sak.LastUsedAt,
                ExpiresAt = sak.ExpiresAt,
                Description = sak.Description,
                RequestsPerMinute = sak.RequestsPerMinute,
                RequestsPerDay = sak.RequestsPerDay
            })
            .ToListAsync();

        return apiKeys;
    }

    public async Task<ApiKeyDto?> GetApiKeyByIdAsync(Guid sellerId, Guid apiKeyId)
    {
        var apiKey = await _context.SellerApiKeys
            .Where(sak => sak.SellerId == sellerId && sak.Id == apiKeyId)
            .Select(sak => new ApiKeyDto
            {
                Id = sak.Id,
                Name = sak.Name,
                ApiKey = sak.ApiKey,
                IsActive = sak.IsActive,
                CreatedAt = sak.CreatedAt,
                LastUsedAt = sak.LastUsedAt,
                ExpiresAt = sak.ExpiresAt,
                Description = sak.Description,
                RequestsPerMinute = sak.RequestsPerMinute,
                RequestsPerDay = sak.RequestsPerDay
            })
            .FirstOrDefaultAsync();

        return apiKey;
    }

    public async Task<Guid?> ValidateApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || !ApiKeyGenerator.IsValidFormat(apiKey))
            return null;

        var sellerApiKey = await _context.SellerApiKeys
            .FirstOrDefaultAsync(sak => sak.ApiKey == apiKey && 
                                      sak.IsActive && 
                                      (sak.ExpiresAt == null || sak.ExpiresAt > DateTime.UtcNow));

        return sellerApiKey?.SellerId;
    }

    public async Task UpdateLastUsedAsync(string apiKey)
    {
        var sellerApiKey = await _context.SellerApiKeys
            .FirstOrDefaultAsync(sak => sak.ApiKey == apiKey);

        if (sellerApiKey != null)
        {
            sellerApiKey.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> DeactivateApiKeyAsync(Guid sellerId, Guid apiKeyId)
    {
        var apiKey = await _context.SellerApiKeys
            .FirstOrDefaultAsync(sak => sak.SellerId == sellerId && sak.Id == apiKeyId);

        if (apiKey == null)
            return false;

        apiKey.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateApiKeyAsync(Guid sellerId, Guid apiKeyId)
    {
        var apiKey = await _context.SellerApiKeys
            .FirstOrDefaultAsync(sak => sak.SellerId == sellerId && sak.Id == apiKeyId);

        if (apiKey == null)
            return false;

        apiKey.IsActive = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteApiKeyAsync(Guid sellerId, Guid apiKeyId)
    {
        var apiKey = await _context.SellerApiKeys
            .FirstOrDefaultAsync(sak => sak.SellerId == sellerId && sak.Id == apiKeyId);

        if (apiKey == null)
            return false;

        _context.SellerApiKeys.Remove(apiKey);
        await _context.SaveChangesAsync();
        return true;
    }
}
