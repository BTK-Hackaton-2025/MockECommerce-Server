namespace MockECommerce.BusinessLayer.Utils;

public class JwtSettings
{
    public string Issuer { get; set; }      // Token’ı hangi issuer üretiyor

    public string Audience { get; set; }    // Token’i hangi audience kullanacak

    public string SecretKey { get; set; }   // Symmetric güvenlik anahtarı

    public int ExpiryMinutes { get; set; }  // Token geçerlilik süresi (dakika)
}