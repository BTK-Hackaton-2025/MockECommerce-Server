using Microsoft.AspNetCore.Identity;

namespace MockECommerce.DAL.Entities;

public class AppRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}