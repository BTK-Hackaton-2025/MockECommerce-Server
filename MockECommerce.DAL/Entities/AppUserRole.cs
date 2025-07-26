using Microsoft.AspNetCore.Identity;

namespace MockECommerce.DAL.Entities;

public class AppUserRole : IdentityUserRole<Guid>
{
    public AppUser? User { get; set; }

    public AppRole? Role { get; set; }
}