using MockECommerce.BusinessLayer.Services;
using System.Security.Claims;

namespace MockECommerce.WebAPI.Middlewares;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthenticationMiddleware(RequestDelegate next, ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        // Only apply to API endpoints that require API key authentication
        var path = context.Request.Path.Value?.ToLower();
        
        // Check if this is an API endpoint that requires API key
        if (path != null && (path.StartsWith("/api/v1/external/") || path.StartsWith("/api/external/")))
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key missing");
                return;
            }

            var apiKey = extractedApiKey.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key is empty");
                return;
            }

            var sellerId = await apiKeyService.ValidateApiKeyAsync(apiKey);
            if (sellerId == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid or expired API Key");
                return;
            }

            // Add seller information to context for use in controllers
            var claims = new List<Claim>
            {
                new Claim("SellerId", sellerId.ToString()!),
                new Claim("AuthType", "ApiKey"),
                new Claim(ClaimTypes.AuthenticationMethod, "ApiKey")
            };

            var identity = new ClaimsIdentity(claims, "ApiKey");
            var principal = new ClaimsPrincipal(identity);
            context.User = principal;

            // Update last used timestamp (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await apiKeyService.UpdateLastUsedAsync(apiKey);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update API key last used timestamp for key: {ApiKey}", 
                        apiKey.Substring(0, Math.Min(apiKey.Length, 10)) + "...");
                }
            });

            _logger.LogInformation("API Key authentication successful for seller: {SellerId}", sellerId);
        }

        await _next(context);
    }
}
