using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockECommerce.BusinessLayer.Services; 
using MockECommerce.DAL.Data;
using MockECommerce.DtoLayer.ApiKeyDtos;
using System.Security.Claims;

namespace MockECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/v1/apikeys")]
public class ApiKeyController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;
    private readonly AppDbContext _context;

    public ApiKeyController(IApiKeyService apiKeyService, AppDbContext context)
    {
        _apiKeyService = apiKeyService;
        _context = context;
    }

    // Note: This assumes seller authentication has been performed 
    // and seller ID is available in claims

    private async Task<Guid> GetSellerIdFromClaimsAsync()
    {
        // First try to get from SellerId claim (for API key auth)
        var sellerIdClaim = User.FindFirst("SellerId")?.Value;
        if (!string.IsNullOrEmpty(sellerIdClaim) && Guid.TryParse(sellerIdClaim, out var sellerId))
            return sellerId;

        // If not found, try to get from JWT claims (user ID) and find seller profile
        var userIdClaim = User.FindFirst("uuid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new InvalidOperationException("User/Seller ID is missing or invalid in claims.");

        // Get seller profile ID from user ID
        var sellerProfile = await _context.SellerProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId);
            
        if (sellerProfile == null)
            throw new InvalidOperationException("Seller profile not found for the current user.");

        return sellerProfile.Id;
    }

    [HttpPost("generate")]
    [Authorize]
    public async Task<IActionResult> GenerateApiKeyAsync([FromBody] CreateApiKeyDto createApiKeyDto)
    {
        var sellerId = await GetSellerIdFromClaimsAsync();
        var apiKey = await _apiKeyService.GenerateApiKeyAsync(sellerId, createApiKeyDto);
        return Ok(apiKey);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetSellerApiKeysAsync()
    {
        var sellerId = await GetSellerIdFromClaimsAsync();
        var apiKeys = await _apiKeyService.GetSellerApiKeysAsync(sellerId);
        return Ok(apiKeys);
    }

    [HttpGet("{apiKeyId}")]
    [Authorize]
    public async Task<IActionResult> GetApiKeyByIdAsync(Guid apiKeyId)
    {
        var sellerId = await GetSellerIdFromClaimsAsync();
        var apiKey = await _apiKeyService.GetApiKeyByIdAsync(sellerId, apiKeyId);
        if (apiKey == null)
            return NotFound();

        return Ok(apiKey);
    }

    [HttpPatch("{apiKeyId}/deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateApiKeyAsync(Guid apiKeyId)
    {
        var sellerId = await GetSellerIdFromClaimsAsync();
        var success = await _apiKeyService.DeactivateApiKeyAsync(sellerId, apiKeyId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("{apiKeyId}/activate")]
    [Authorize]
    public async Task<IActionResult> ActivateApiKeyAsync(Guid apiKeyId)
    {
        var sellerId = await GetSellerIdFromClaimsAsync();
        var success = await _apiKeyService.ActivateApiKeyAsync(sellerId, apiKeyId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{apiKeyId}")]
    [Authorize]
    public async Task<IActionResult> DeleteApiKeyAsync(Guid apiKeyId)
    {
        var sellerId = await GetSellerIdFromClaimsAsync();
        var success = await _apiKeyService.DeleteApiKeyAsync(sellerId, apiKeyId);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
