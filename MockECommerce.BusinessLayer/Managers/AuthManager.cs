using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MockECommerce.BusinessLayer.Services;
using MockECommerce.BusinessLayer.Utils;
using MockECommerce.DAL.Data;
using MockECommerce.DAL.Entities;
using MockECommerce.DtoLayer.AuthDtos;

namespace MockECommerce.BusinessLayer.Managers;

public class AuthManager : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthManager(UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        AppDbContext context,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> RegisterUserAsync(RegisterUserDto registerUserDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerUserDto.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "Bu kullanıcı adı zaten kayıtlı."
            };
        }

        // 2) Yeni kullanıcı nesnesi
        var newUser = new AppUser()
        {
            UserName    = registerUserDto.Email,
            FirstName    = registerUserDto.FirstName,
            LastName   = registerUserDto.LastName,
            Email       = registerUserDto.Email,
        };

        // 3) Kullanıcı oluşturma
        var createUserResult = await _userManager.CreateAsync(newUser, registerUserDto.Password);
        if (!createUserResult.Succeeded)
        {
            // Her bir hatanın Description’ını birleştiriyoruz
            var errorMessages = createUserResult.Errors
                .Select(e => e.Description);

            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = string.Join(" | ", errorMessages)
            };
        }

        // 4) Rol var mı kontrol et, yoksa ekle
        if (!await _roleManager.RoleExistsAsync(registerUserDto.Role))
        {
            var roleResult = await _roleManager.CreateAsync(new AppRole { Name = registerUserDto.Role });
            if (!roleResult.Succeeded)
            {
                var roleErrors = roleResult.Errors.Select(e => e.Description);
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Message = string.Join(" | ", roleErrors)
                };
            }
        }

        // 5) Rol ataması
        var addToRoleResult = await _userManager.AddToRoleAsync(newUser, registerUserDto.Role);
        if (!addToRoleResult.Succeeded)
        {
            var roleAssignErrors = addToRoleResult.Errors.Select(e => e.Description);
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = string.Join(" | ", roleAssignErrors)
            };
        }

        // 6) Başarılı olunca JWT oluştur
        var token = await GenerateJwtTokenAsync(newUser);

        return new AuthResponseDto
        {
            IsAuthenticated = true,
            Token = token,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Email = newUser.Email,
            Role = "User",
            Message = "Kayıt başarılı."
        };
    }

    public async Task<AuthResponseDto> RegisterSellerAsync(RegisterSellerDto registerSellerDto)
    {
        // 1) Kullanıcı zaten var mı kontrol et
        var existingUser = await _userManager.FindByEmailAsync(registerSellerDto.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "Bu e-posta adresi zaten kayıtlı."
            };
        }

        // 2) Yeni kullanıcı nesnesi oluştur
        var newUser = new AppUser()
        {
            UserName = registerSellerDto.Email,
            FirstName = registerSellerDto.FirstName,
            LastName = registerSellerDto.LastName,
            Email = registerSellerDto.Email,
        };

        // 3) Kullanıcı oluşturma
        var createUserResult = await _userManager.CreateAsync(newUser, registerSellerDto.Password);
        if (!createUserResult.Succeeded)
        {
            var errorMessages = createUserResult.Errors.Select(e => e.Description);
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = string.Join(" | ", errorMessages)
            };
        }

        // 4) "Seller" rolü var mı kontrol et, yoksa ekle
        const string sellerRole = "Seller";
        if (!await _roleManager.RoleExistsAsync(sellerRole))
        {
            var roleResult = await _roleManager.CreateAsync(new AppRole { Name = sellerRole });
            if (!roleResult.Succeeded)
            {
                var roleErrors = roleResult.Errors.Select(e => e.Description);
                return new AuthResponseDto
                {
                    IsAuthenticated = false,
                    Message = string.Join(" | ", roleErrors)
                };
            }
        }

        // 5) Seller rolü ataması
        var addToRoleResult = await _userManager.AddToRoleAsync(newUser, sellerRole);
        if (!addToRoleResult.Succeeded)
        {
            var roleAssignErrors = addToRoleResult.Errors.Select(e => e.Description);
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = string.Join(" | ", roleAssignErrors)
            };
        }

        // 6) SellerProfile oluştur
        var sellerProfile = new SellerProfile
        {
            Id = Guid.NewGuid(),
            UserId = newUser.Id,
            StoreName = registerSellerDto.StoreName,
            LogoUrl = registerSellerDto.LogoUrl,
            IsApproved = false, // Varsayılan olarak onaylanmamış
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _context.SellerProfiles.AddAsync(sellerProfile);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Hata durumunda kullanıcıyı da silebiliriz
            await _userManager.DeleteAsync(newUser);
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = $"Seller profili oluşturulurken hata oluştu: {ex.Message}"
            };
        }

        // 7) JWT token oluştur
        var token = await GenerateJwtTokenAsync(newUser);

        return new AuthResponseDto
        {
            IsAuthenticated = true,
            Token = token,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Email = newUser.Email,
            Role = sellerRole,
            Message = "Seller kayıt başarılı. Hesabınız onay bekliyor."
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // 1) Kullanıcı adından kullanıcıyı bul
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "Kullanıcı adı veya şifre hatalı."
            };
        }

        // 2) Şifre doğru mu kontrol et
        var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "Kullanıcı adı veya şifre hatalı."
            };
        }

        // 3) Kullanıcının rolünü al
        var userRoles = await _userManager.GetRolesAsync(user);
        var role = userRoles.Count > 0 ? userRoles[0] : string.Empty;

        // 4) JWT oluştur
        var token = await GenerateJwtTokenAsync(user);

        return new AuthResponseDto
        {
            IsAuthenticated = true,
            Token = token,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = role,
            Message = "Giriş başarılı."
        };
    }

    private async Task<string> GenerateJwtTokenAsync(AppUser user)
    {
        // 1) Kullanıcının IdentityClaim’lerini (rol+ kimlik) topla
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        var roleClaims = new List<Claim>();
        foreach (var role in roles)
        {
            roleClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        // 2) Token için temel Claim'ler
        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),        // Subject (User ID)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),    // Token'ın benzersizliği
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim("uuid", user.Id.ToString())
            }
            .Union(userClaims)       // varsa extra claim'ler
            .Union(roleClaims);      // rol claim'leri

        // 3) JWT anahtarını al
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 4) Token yaratıcısı
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}