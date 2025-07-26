using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MockECommerce.DAL.Data;
using MockECommerce.DAL.Entities;

namespace MockECommerce.WebAPI.Extensions;

public static class DatabaseSeedExtensions
{
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            
            // Seed roles
            await SeedRolesAsync(roleManager, logger);
            
            // Seed admin user
            await SeedAdminUserAsync(userManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager, ILogger logger)
    {
        var roles = new[]
        {
            new AppRole { Name = "Admin", Description = "System Administrator" },
            new AppRole { Name = "Seller", Description = "Product Seller" },
            new AppRole { Name = "Customer", Description = "Regular Customer" }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    logger.LogInformation("Role '{RoleName}' created successfully", role.Name);
                }
                else
                {
                    logger.LogError("Failed to create role '{RoleName}': {Errors}", 
                        role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Role '{RoleName}' already exists", role.Name);
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager, ILogger logger)
    {
        const string adminEmail = "admin@example.com";
        const string adminPassword = "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            
            if (result.Succeeded)
            {
                // Add admin role
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Admin user created successfully with email: {Email}", adminEmail);
                    logger.LogInformation("Default admin password is: {Password}", adminPassword);
                    logger.LogWarning("IMPORTANT: Change the default admin password after first login!");
                }
                else
                {
                    logger.LogError("Failed to add Admin role to user: {Errors}", 
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists: {Email}", adminEmail);
            
            // Ensure admin has Admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Added Admin role to existing user: {Email}", adminEmail);
                }
            }
        }
    }
}
