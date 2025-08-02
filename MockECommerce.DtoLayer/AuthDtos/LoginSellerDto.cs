using System.ComponentModel.DataAnnotations;

namespace MockECommerce.DtoLayer.AuthDtos;

public class LoginSellerDto
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}