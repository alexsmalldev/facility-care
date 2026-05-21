using FacilityCare.Application.DTOs.Buildings;
using FacilityCare.Infrastructure.Persistence;
using FacilityCare.Infrastructure.Services;
using FacilityCare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FacilityCare.Application.Tests.Services;

public class BuildingServiceTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly AppDbContext _context;
    private readonly BuildingService _buildingService;

    public BuildingServiceTests()
    {
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _buildingService = new BuildingService(_context, _userManagerMock.Object);

        SeedData();
    }

    private void SeedData()
    {
        _context.Buildings.AddRange(
            new Building
            {
                Id = 1,
                Name = "Test Building",
                AddressLine1 = "123 Test St",
                City = "Test City",
                Postcode = "12345",
                Latitude = 51.5074m,
                Longitude = -0.1278m,
                BuildingUsers = new List<BuildingUser>
                {
                    new BuildingUser { BuildingId = 1, UserId = "regular-user-id" }
                }
            },
            new Building
            {
                Id = 2,
                Name = "B Building",
                AddressLine1 = "456 B St",
                City = "B City",
                Postcode = "22222",
                Latitude = 52.5074m,
                Longitude = -0.2278m
            },
            new Building
            {
                Id = 3,
                Name = "A Building",
                AddressLine1 = "789 A St",
                City = "A City",
                Postcode = "33333",
                Latitude = 53.5074m,
                Longitude = -0.3278m
            }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_AsAdmin_ReturnsAllBuildings()
    {
        var result = await _buildingService.GetAllAsync("admin-id", isAdmin: true);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_AsRegularUser_ReturnsOnlyAssignedBuildings()
    {
        var result = await _buildingService.GetAllAsync("regular-user-id", isAdmin: false);
        Assert.Single(result);
        Assert.Equal("Test Building", result[0].Name);
    }

    [Fact]
    public async Task GetAllAsync_AsRegularUser_WithNoAssignedBuildings_ReturnsEmpty()
    {
        var result = await _buildingService.GetAllAsync("unassigned-user-id", isAdmin: false);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBuildingNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _buildingService.GetByIdAsync(999, "admin-id", isAdmin: true));
    }

    [Fact]
    public async Task GetByIdAsync_AsAdmin_ReturnsBuilding()
    {
        var result = await _buildingService.GetByIdAsync(1, "admin-id", isAdmin: true);
        Assert.Equal("Test Building", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_AsRegularUser_WhenNotAssigned_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _buildingService.GetByIdAsync(2, "regular-user-id", isAdmin: false));
    }

    [Fact]
    public async Task GetByIdAsync_AsRegularUser_WhenAssigned_ReturnsBuilding()
    {
        var result = await _buildingService.GetByIdAsync(1, "regular-user-id", isAdmin: false);
        Assert.Equal("Test Building", result.Name);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedBuilding()
    {
        var result = await _buildingService.CreateAsync(new CreateBuildingRequest
        {
            Name = "New Building",
            AddressLine1 = "456 New St",
            City = "New City",
            Postcode = "54321",
            Country = "United Kingdom",
            Latitude = 52.5200m,
            Longitude = 13.4050m
        });

        Assert.Equal("New Building", result.Name);
        Assert.Equal(4, await _context.Buildings.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_WhenBuildingNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _buildingService.UpdateAsync(999, new UpdateBuildingRequest()));
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ReturnsUpdatedBuilding()
    {
        var result = await _buildingService.UpdateAsync(1, new UpdateBuildingRequest
        {
            Name = "Updated Building",
            City = "Updated City"
        });

        Assert.Equal("Updated Building", result.Name);
        Assert.Equal("Updated City", result.City);
    }

    [Fact]
    public async Task DeleteAsync_WhenBuildingNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _buildingService.DeleteAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesBuilding()
    {
        await _buildingService.DeleteAsync(1);
        Assert.Equal(2, await _context.Buildings.CountAsync());
    }

    [Fact]
    public async Task GetRegistrationListAsync_ReturnsAllBuildings()
    {
        var result = await _buildingService.GetRegistrationListAsync();
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task UpdateBuildingUsersAsync_WhenBuildingNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _buildingService.UpdateBuildingUsersAsync(999, new UpdateBuildingUsersRequest
            {
                UserIds = new List<string> { "user-id" }
            }));
    }

    [Fact]
    public async Task UpdateBuildingUsersAsync_WithInvalidUserId_ThrowsException()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("invalid-id"))
            .ReturnsAsync((IdentityUser?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _buildingService.UpdateBuildingUsersAsync(1, new UpdateBuildingUsersRequest
            {
                UserIds = new List<string> { "invalid-id" }
            }));
    }

    [Fact]
    public async Task UpdateBuildingUsersAsync_WithValidUserIds_UpdatesUsers()
    {
        var user = new IdentityUser { Id = "new-user-id", UserName = "newuser" };
        _userManagerMock.Setup(u => u.FindByIdAsync("new-user-id"))
            .ReturnsAsync(user);

        await _buildingService.UpdateBuildingUsersAsync(1, new UpdateBuildingUsersRequest
        {
            UserIds = new List<string> { "new-user-id" }
        });

        var buildingUsers = await _context.BuildingUsers
            .Where(bu => bu.BuildingId == 1)
            .ToListAsync();

        Assert.Single(buildingUsers);
        Assert.Equal("new-user-id", buildingUsers[0].UserId);
    }
}