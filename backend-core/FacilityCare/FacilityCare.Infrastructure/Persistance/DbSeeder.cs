using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FacilityCare.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Seed roles
        foreach (var role in new[] { "Admin", "Regular" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed admin 
        var existingUser = await userManager.FindByNameAsync("admin");
        if (existingUser != null) return;

        var user = new IdentityUser
        {
            UserName = "admin",
            Email = "admin@facilitycare.com",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(user, "Admin123!");
        await userManager.AddToRoleAsync(user, "Admin");
        
        // Seed regular 
        var existingReegularUser = await userManager.FindByNameAsync("regular");
        if (existingReegularUser != null) return;

        var regularUser = new IdentityUser
        {
            UserName = "regular",
            Email = "regular@facilitycare.com",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(regularUser, "Regular123!");
        await userManager.AddToRoleAsync(regularUser, "Regular");
    }
}