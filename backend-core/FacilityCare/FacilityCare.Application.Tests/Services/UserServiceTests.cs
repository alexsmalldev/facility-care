using FacilityCare.Application.DTOs.Users;
using FacilityCare.Infrastructure.Persistence;
using FacilityCare.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FacilityCare.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly AppDbContext _context;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

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
            .ReturnsAsync((IdentityUser?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _userService.GetByIdAsync("invalid-id"));
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserFound_ReturnsUserDto()
    {
        var user = new IdentityUser { Id = "1", UserName = "testuser", Email = "test@test.com" };

        _userManagerMock.Setup(u => u.FindByIdAsync("1"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Regular" });

        var result = await _userService.GetByIdAsync("1");

        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@test.com", result.Email);
        Assert.Contains("Regular", result.Roles);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_ThrowsException()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("invalid-id"))
            .ReturnsAsync((IdentityUser?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _userService.UpdateAsync("invalid-id", new UpdateUserRequest()));
    }

    [Fact]
    public async Task UpdateAsync_WhenValidRequest_ReturnsUpdatedUserDto()
    {
        var user = new IdentityUser { Id = "1", UserName = "oldname", Email = "old@test.com" };

        _userManagerMock.Setup(u => u.FindByIdAsync("1"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(u => u.UpdateAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(new List<string> { "Regular" });

        var result = await _userService.UpdateAsync("1", new UpdateUserRequest
        {
            Username = "newname",
            Email = "new@test.com"
        });

        Assert.Equal("newname", result.Username);
        Assert.Equal("new@test.com", result.Email);
    }

    [Fact]
    public async Task AssignBuildingsAsync_WhenUserNotFound_ThrowsException()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("invalid-id"))
            .ReturnsAsync((IdentityUser?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _userService.AssignBuildingsAsync("invalid-id", new AssignBuildingsRequest
            {
                BuildingIds = new List<int> { 1 }
            }));
    }

    [Fact]
    public async Task AssignBuildingsAsync_WhenBuildingNotFound_ThrowsException()
    {
        var user = new IdentityUser { Id = "1", UserName = "testuser" };

        _userManagerMock.Setup(u => u.FindByIdAsync("1"))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<Exception>(() =>
            _userService.AssignBuildingsAsync("1", new AssignBuildingsRequest
            {
                BuildingIds = new List<int> { 999 }
            }));
    }
}