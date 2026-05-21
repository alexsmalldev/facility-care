using FacilityCare.Application.DTOs.Users;
using FacilityCare.Infrastructure.Persistence;
using FacilityCare.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FacilityCare.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly AppDbContext _context;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _userService = new UserService(_userManagerMock.Object, _context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserNotFound_ThrowsException()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("invalid-id"))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _userService.GetByIdAsync("invalid-id"));
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserFound_ReturnsUserDto()
    {
        var user = new ApplicationUser { Id = "1", UserName = "testuser", Email = "test@test.com", FirstName = "Test", LastName = "User" };

        _userManagerMock.Setup(u => u.FindByIdAsync("1"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Regular" });

        var result = await _userService.GetByIdAsync("1");

        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.Contains("Regular", result.Roles);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_ThrowsException()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("invalid-id"))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _userService.UpdateAsync("invalid-id", new UpdateUserRequest()));
    }

    [Fact]
    public async Task UpdateAsync_WhenValidRequest_ReturnsUpdatedUserDto()
    {
        var user = new ApplicationUser { Id = "1", UserName = "oldname", Email = "old@test.com", FirstName = "Old", LastName = "Name" };

        _userManagerMock.Setup(u => u.FindByIdAsync("1"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(u => u.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Regular" });

        var result = await _userService.UpdateAsync("1", new UpdateUserRequest
        {
            Username = "newname",
            Email = "new@test.com",
            FirstName = "New",
            LastName = "Name"
        });

        Assert.Equal("newname", result.Username);
        Assert.Equal("new@test.com", result.Email);
        Assert.Equal("New", result.FirstName);
        Assert.Equal("Name", result.LastName);
    }

    [Fact]
    public async Task AssignBuildingsAsync_WhenUserNotFound_ThrowsException()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("invalid-id"))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _userService.AssignBuildingsAsync("invalid-id", new AssignBuildingsRequest
            {
                BuildingIds = new List<int> { 1 }
            }));
    }

    [Fact]
    public async Task AssignBuildingsAsync_WhenBuildingNotFound_ThrowsException()
    {
        var user = new ApplicationUser { Id = "1", UserName = "testuser" };

        _userManagerMock.Setup(u => u.FindByIdAsync("1"))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<Exception>(() =>
            _userService.AssignBuildingsAsync("1", new AssignBuildingsRequest
            {
                BuildingIds = new List<int> { 999 }
            }));
    }
}