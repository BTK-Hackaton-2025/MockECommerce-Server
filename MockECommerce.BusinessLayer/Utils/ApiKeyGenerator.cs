using System.Security.Cryptography;
using System.Text;

namespace MockECommerce.BusinessLayer.Utils;

public static class ApiKeyGenerator
{
    private const string Prefix = "mec_"; // MockECommerce prefix
    private const int KeyLength = 32; // 32 bytes = 256 bits
    
    
    /// Generates a cryptographically secure API key
    /// Format: mec_[base64-encoded-random-bytes]
    
    /// <returns>A unique API key</returns>
    public static string GenerateApiKey()
    {
        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[KeyLength];
        rng.GetBytes(randomBytes);
        
        var base64String = Convert.ToBase64String(randomBytes)
            .Replace("+", "-")  // URL-safe
            .Replace("/", "_")  // URL-safe
            .Replace("=", "");  // Remove padding
        
        return $"{Prefix}{base64String}";
    }
    
   
    /// Validates API key format
    
    /// <param name="apiKey">The API key to validate</param>
    /// <returns>True if format is valid</returns>
    public static bool IsValidFormat(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return false;
            
        if (!apiKey.StartsWith(Prefix))
            return false;
            
        var keyPart = apiKey.Substring(Prefix.Length);
        
        // Check if it's a valid base64 string (URL-safe variant)
        if (keyPart.Length == 0)
            return false;
            
        // Base64 characters plus URL-safe replacements
        return keyPart.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }
}
