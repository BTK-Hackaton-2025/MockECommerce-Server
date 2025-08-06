using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MockECommerce.DAL.Entities;

namespace MockECommerce.DAL.Data;

public class AppDbContext
    : IdentityDbContext<AppUser, AppRole, Guid,
        IdentityUserClaim<Guid>, AppUserRole,
        IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<SellerProfile>         SellerProfiles         { get; set; }
    public DbSet<SellerApiKey>          SellerApiKeys          { get; set; }
    public DbSet<Category>              Categories             { get; set; }
    public DbSet<Product>               Products               { get; set; }
    public DbSet<Order>                 Orders                 { get; set; }
    public DbSet<ProductImage>          ProductImages          { get; set; }
    public DbSet<MarketplaceCredential> MarketplaceCredentials { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // ---------- Configuration Methods ----------
        ConfigureIdentityTables(b);
        ConfigureSellerProfile(b);
        ConfigureSellerApiKey(b);
        ConfigureProduct(b);
        ConfigureOrder(b);
        ConfigureCategory(b);
        ConfigureProductImage(b);
        ConfigureMarketplaceCredential(b);
    }

    private void ConfigureIdentityTables(ModelBuilder b)
    {
        // ---------- Identity tablo adları ----------
        b.Entity<AppUser>().ToTable("Users");
        b.Entity<AppRole>().ToTable("Roles");
        b.Entity<AppUserRole>().ToTable("UserRoles");
        b.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        b.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        b.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        b.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // PK + FK’leri de explicit tanımla
        b.Entity<AppUserRole>(entity =>
        {
            // Birincil anahtar
            entity.HasKey(x => new { x.UserId, x.RoleId });

            // User–UserRoles
            entity.HasOne(x => x.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(x => x.UserId)
                .IsRequired();

            // Role–UserRoles
            entity.HasOne(x => x.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .IsRequired();
        });
    }

    private void ConfigureSellerProfile(ModelBuilder b)
        {
            b.Entity<SellerProfile>(entity =>
            {
                entity.ToTable("SellerProfiles");
                entity.HasKey(x => x.Id);

                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                // ---------- 1-1  User ↔ SellerProfile ----------
                entity.HasOne(sp => sp.User)
                      .WithOne(u => u.SellerProfile)
                      .HasForeignKey<SellerProfile>(sp => sp.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureSellerApiKey(ModelBuilder b)
        {
            b.Entity<SellerApiKey>(entity =>
            {
                entity.ToTable("SellerApiKeys");
                entity.HasKey(x => x.Id);

                // ---------- Indexes ----------
                entity.HasIndex(sak => sak.ApiKey)
                      .IsUnique()
                      .HasDatabaseName("IX_SellerApiKeys_ApiKey");
                      
                entity.HasIndex(sak => sak.SellerId)
                      .HasDatabaseName("IX_SellerApiKeys_SellerId");

                // ---------- 1-M  SellerProfile ↔ SellerApiKey ----------
                entity.HasOne(sak => sak.Seller)
                      .WithMany(s => s.ApiKeys)
                      .HasForeignKey(sak => sak.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureProduct(ModelBuilder b)
        {
            b.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(x => x.Id);

                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                // ---------- Indexes for Repository Performance ----------
                entity.HasIndex(p => p.SellerId)
                      .HasDatabaseName("IX_Products_SellerId");

                entity.HasIndex(p => p.CategoryId)
                      .HasDatabaseName("IX_Products_CategoryId");

                // ---------- 1-N  SellerProfile ↔ Product ----------
                entity.HasOne(p => p.Seller)
                      .WithMany(s => s.Products)
                      .HasForeignKey(p => p.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ---------- 1-N  Category ↔ Product ----------
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureOrder(ModelBuilder b)
        {
            b.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(x => x.Id);

                entity.Property(o => o.OrderDate)
                      .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                // ---------- Status column configuration ----------
                entity.Property(o => o.Status)
                      .HasMaxLength(50)
                      .IsRequired();

                // ---------- Indexes for Order queries ----------
                entity.HasIndex(o => o.ProductId)
                      .HasDatabaseName("IX_Orders_ProductId");

                entity.HasIndex(o => o.CustomerId)
                      .HasDatabaseName("IX_Orders_CustomerId");

                // ---------- 1-N  Product ↔ Order ----------
                entity.HasOne(o => o.Product)
                      .WithMany()
                      .HasForeignKey(o => o.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ---------- 1-N  AppUser ↔ Order ----------
                entity.HasOne(o => o.Customer)
                      .WithMany()
                      .HasForeignKey(o => o.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureCategory(ModelBuilder b)
        {
            b.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(x => x.Id);

                // ---------- Index for Parent-Child queries ----------
                entity.HasIndex(c => c.ParentId)
                      .HasDatabaseName("IX_Categories_ParentId");

                // ---------- Self-ref. Category (Parent-Child) ----------
                entity.HasMany(c => c.Children)
                      .WithOne(c => c.Parent)
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureProductImage(ModelBuilder b)
        {
            b.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImages");
                entity.HasKey(x => x.Id);

                // ---------- Index for Product queries ----------
                entity.HasIndex(pi => pi.ProductId)
                      .HasDatabaseName("IX_ProductImages_ProductId");

                // ---------- 1-M Product ↔ ProductImage ----------
                entity.HasOne(pi => pi.Product)
                      .WithMany(p => p.Images)
                      .HasForeignKey(pi => pi.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureMarketplaceCredential(ModelBuilder b)
        {
            b.Entity<MarketplaceCredential>(entity =>
            {
                entity.ToTable("MarketplaceCredentials");
                entity.HasKey(x => x.Id);

                // ---------- Index for Seller queries ----------
                entity.HasIndex(mc => mc.SellerId)
                      .HasDatabaseName("IX_MarketplaceCredentials_SellerId");

                // ---------- 1-M  SellerProfile ↔ MarketplaceCredential ----------
                entity.HasOne(mc => mc.Seller)
                      .WithMany(s => s.Credentials)
                      .HasForeignKey(mc => mc.SellerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
}