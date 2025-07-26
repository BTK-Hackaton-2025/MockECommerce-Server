namespace MockECommerce.DtoLayer.AuthDtos;

public class AuthResponseDto
{
    public bool IsAuthenticated { get; set; }
    public string Token { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string Message { get; set; }
}