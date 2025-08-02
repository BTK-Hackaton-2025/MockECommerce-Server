using MockECommerce.DtoLayer.AuthDtos;

namespace MockECommerce.BusinessLayer.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterUserAsync(RegisterUserDto registerUserDto);
    Task<AuthResponseDto> RegisterSellerAsync(RegisterSellerDto registerSellerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> LoginSellerAsync(LoginSellerDto loginSellerDto);
}
