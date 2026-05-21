using FacilityCare.Domain.Entities;
using FacilityCare.Domain.Enums;
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

        var admin = await userManager.FindByNameAsync("admin");
        var regularSeed = await userManager.FindByNameAsync("regular");

        if (!context.ServiceRequests.Any() && admin != null && regularSeed != null)
        {
            var cleaning = context.ServiceTypes.First(st => st.Name == "Cleaning");
            var maintenance = context.ServiceTypes.First(st => st.Name == "Maintenance");
            var security = context.ServiceTypes.First(st => st.Name == "Security");
            var hq = context.Buildings.First(b => b.Name == "Headquarters");
            var northOffice = context.Buildings.First(b => b.Name == "North Office");

            var requests = new List<ServiceRequest>
            {
                new ServiceRequest
                {
                    CustomerNotes = "Common areas need a deep clean",
                    Status = ServiceRequestStatus.Open,
                    Priority = ServiceRequestPriority.Low,
                    CreatedById = regularSeed.Id,
                    ServiceTypeId = cleaning.Id,
                    BuildingId = hq.Id,
                    ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(5)
                },
                new ServiceRequest
                {
                    CustomerNotes = "Boiler making strange noise",
                    Status = ServiceRequestStatus.InProgress,
                    Priority = ServiceRequestPriority.High,
                    CreatedById = regularSeed.Id,
                    ServiceTypeId = maintenance.Id,
                    BuildingId = hq.Id,
                    ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(1)
                },
                new ServiceRequest
                {
                    CustomerNotes = "Access card not working",
                    Status = ServiceRequestStatus.Completed,
                    Priority = ServiceRequestPriority.Medium,
                    CreatedById = regularSeed.Id,
                    ServiceTypeId = security.Id,
                    BuildingId = northOffice.Id,
                    ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(3)
                },
                new ServiceRequest
                {
                    CustomerNotes = "Office windows need cleaning",
                    Status = ServiceRequestStatus.Open,
                    Priority = ServiceRequestPriority.Low,
                    CreatedById = regularSeed.Id,
                    ServiceTypeId = cleaning.Id,
                    BuildingId = northOffice.Id,
                    ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(5)
                },
                new ServiceRequest
                {
                    CustomerNotes = "Ceiling light flickering in meeting room",
                    Status = ServiceRequestStatus.InProgress,
                    Priority = ServiceRequestPriority.Medium,
                    CreatedById = regularSeed.Id,
                    ServiceTypeId = maintenance.Id,
                    BuildingId = hq.Id,
                    ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(3)
                }
            };

            context.ServiceRequests.AddRange(requests);
            await context.SaveChangesAsync();

            var updates = new List<Update>
            {
                new Update
                {
                    Title = "Request Created",
                    Message = "Request has been created",
                    CreatedById = regularSeed.Id,
                    ServiceRequestId = requests[0].Id,
                    Type = UpdateType.Event,
                    IsRead = true
                },
                new Update
                {
                    Title = "Request Created",
                    Message = "Request has been created",
                    CreatedById = regularSeed.Id,
                    ServiceRequestId = requests[1].Id,
                    Type = UpdateType.Event,
                    IsRead = true
                },
                new Update
                {
                    Title = $"Request {requests[1].Id} Status Update",
                    Message = $"Request {requests[1].Id} has been moved to InProgress",
                    CreatedById = admin.Id,
                    AssociatedToId = regularSeed.Id,
                    ServiceRequestId = requests[1].Id,
                    Type = UpdateType.Event,
                    IsRead = false
                },
                new Update
                {
                    Title = $"A Comment has been added to Request {requests[1].Id}",
                    Message = "Engineer has been dispatched to inspect the boiler",
                    CreatedById = admin.Id,
                    AssociatedToId = regularSeed.Id,
                    ServiceRequestId = requests[1].Id,
                    Type = UpdateType.Message,
                    IsRead = false
                },
                new Update
                {
                    Title = "Request Created",
                    Message = "Request has been created",
                    CreatedById = regularSeed.Id,
                    ServiceRequestId = requests[2].Id,
                    Type = UpdateType.Event,
                    IsRead = true
                },
                new Update
                {
                    Title = $"Request {requests[2].Id} Status Update",
                    Message = $"Request {requests[2].Id} has been moved to Completed",
                    CreatedById = admin.Id,
                    AssociatedToId = regularSeed.Id,
                    ServiceRequestId = requests[2].Id,
                    Type = UpdateType.Event,
                    IsRead = true
                },
                new Update
                {
                    Title = $"A Comment has been added to Request {requests[2].Id}",
                    Message = "Access card has been reprogrammed and tested successfully",
                    CreatedById = admin.Id,
                    AssociatedToId = regularSeed.Id,
                    ServiceRequestId = requests[2].Id,
                    Type = UpdateType.Message,
                    IsRead = true
                },
                new Update
                {
                    Title = "Request Created",
                    Message = "Request has been created",
                    CreatedById = regularSeed.Id,
                    ServiceRequestId = requests[3].Id,
                    Type = UpdateType.Event,
                    IsRead = true
                },
                new Update
                {
                    Title = "Request Created",
                    Message = "Request has been created",
                    CreatedById = regularSeed.Id,
                    ServiceRequestId = requests[4].Id,
                    Type = UpdateType.Event,
                    IsRead = true
                },
                new Update
                {
                    Title = $"A Comment has been added to Request {requests[4].Id}",
                    Message = "Electrician scheduled for tomorrow morning",
                    CreatedById = admin.Id,
                    AssociatedToId = regularSeed.Id,
                    ServiceRequestId = requests[4].Id,
                    Type = UpdateType.Message,
                    IsRead = false
                }
            };

            context.Updates.AddRange(updates);
            await context.SaveChangesAsync();
        }
    }
}