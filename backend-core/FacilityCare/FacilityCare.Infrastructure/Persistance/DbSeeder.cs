using FacilityCare.Domain.Entities;
using FacilityCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FacilityCare.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        foreach (var role in new[] { "Admin", "Regular" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var existingUser = await userManager.FindByNameAsync("admin");
        if (existingUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@facilitycare.com",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User"
            };
            await userManager.CreateAsync(user, "Admin123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }

        var existingRegularUser = await userManager.FindByNameAsync("regular");
        if (existingRegularUser == null)
        {
            var regularUser = new ApplicationUser
            {
                UserName = "regular",
                Email = "regular@facilitycare.com",
                EmailConfirmed = true,
                FirstName = "Regular",
                LastName = "User"
            };
            await userManager.CreateAsync(regularUser, "Regular123!");
            await userManager.AddToRoleAsync(regularUser, "Regular");
        }

        if (!context.ServiceTypes.Any())
        {
            context.ServiceTypes.AddRange(
                new ServiceType { Name = "Cleaning", Description = "General cleaning and janitorial services", IsActive = true },
                new ServiceType { Name = "Maintenance", Description = "General building maintenance and repairs", IsActive = true },
                new ServiceType { Name = "Security", Description = "Security services and access control", IsActive = true },
                new ServiceType { Name = "IT Support", Description = "IT infrastructure and technical support", IsActive = true },
                new ServiceType { Name = "Catering", Description = "Food and beverage services", IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Buildings.Any())
        {
            context.Buildings.AddRange(
                new Building { Name = "Headquarters", AddressLine1 = "1 Main Street", City = "London", Postcode = "EC1A 1BB", Country = "United Kingdom", Latitude = 51.5074m, Longitude = -0.1278m },
                new Building { Name = "North Office", AddressLine1 = "45 King Street", City = "Manchester", Postcode = "M2 4LQ", Country = "United Kingdom", Latitude = 53.4808m, Longitude = -2.2426m },
                new Building { Name = "South Office", AddressLine1 = "12 Queens Road", City = "Brighton", Postcode = "BN1 3WB", Country = "United Kingdom", Latitude = 50.8225m, Longitude = -0.1372m }
            );
            await context.SaveChangesAsync();
        }

        var regular = await userManager.FindByNameAsync("regular");
        var headquarters = context.Buildings.FirstOrDefault(b => b.Name == "Headquarters");
        if (regular != null && headquarters != null && !context.BuildingUsers.Any())
        {
            context.BuildingUsers.Add(new BuildingUser { BuildingId = headquarters.Id, UserId = regular.Id });
            await context.SaveChangesAsync();
        }
    }
}