using Microsoft.AspNetCore.Identity;

namespace MockECommerce.DAL.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // — Navigasyon —
    public SellerProfile? SellerProfile { get; set; }
    // public ICollection<Address> Addresses { get; set; } = new List<Address>();
    // public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}