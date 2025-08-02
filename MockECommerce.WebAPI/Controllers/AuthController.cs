using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MockECommerce.BusinessLayer.Services;
using MockECommerce.DtoLayer.AuthDtos;
using System.Security.Claims;

namespace MockECommerce.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserDto registerUserDto)
    {
        if (registerUserDto == null)
            return BadRequest("Kayıt bilgileri eksik.");

        var result = await _authService.RegisterUserAsync(registerUserDto);
        if (!result.IsAuthenticated)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("register-seller")]
    public async Task<IActionResult> RegisterSellerAsync([FromBody] RegisterSellerDto registerSellerDto)
    {
        if (registerSellerDto == null)
            return BadRequest("Satıcı kayıt bilgileri eksik.");

        var result = await _authService.RegisterSellerAsync(registerSellerDto);
        if (!result.IsAuthenticated)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
    {
        if (loginDto == null)
            return BadRequest("Giriş bilgileri eksik.");

        var result = await _authService.LoginAsync(loginDto);
        if (!result.IsAuthenticated)
            return Unauthorized(result.Message);

        return Ok(result);
    }

    [HttpPost("login-seller")]
    public async Task<IActionResult> LoginSellerAsync([FromBody] LoginSellerDto loginSellerDto)
    {
        if (loginSellerDto == null)
            return BadRequest("Giriş bilgileri eksik.");

        var result = await _authService.LoginSellerAsync(loginSellerDto);
        if (!result.IsAuthenticated)
            return Unauthorized(result.Message);

        return Ok(result);
    }

    /// <summary>
    /// Test endpoint to verify token and see user claims
    /// </summary>
    [HttpGet("test-token")]
    [Authorize]
    public IActionResult TestToken()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var isAdmin = User.IsInRole("Admin");
        
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            UserName = User.Identity?.Name,
            Claims = claims,
            Roles = roles,
            IsAdmin = isAdmin,
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        });
    }
}
