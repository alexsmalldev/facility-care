using FacilityCare.Application.DTOs.Auth;
using FacilityCare.Infrastructure.Persistence;
using FacilityCare.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FacilityCare.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Jwt:Key", "this-is-a-test-secret-key-32chars!!" },
                { "Jwt:Issuer", "FacilityCare" },
                { "Jwt:Audience", "FacilityCare" }
            }!)
            .Build();

        _authService = new AuthService(_userManagerMock.Object, configuration);
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameAlreadyExists_ThrowsException()
    {
        _userManagerMock.Setup(u => u.FindByNameAsync("admin"))
            .ReturnsAsync(new ApplicationUser { UserName = "admin" });

        await Assert.ThrowsAsync<Exception>(() =>
            _authService.RegisterAsync(new RegisterRequest
            {
                Username = "admin",
                Email = "admin@test.com",
                Password = "Admin123!"
            }));
    }

    [Fact]
    public async Task RegisterAsync_WhenValidRequest_ReturnsAuthResponse()
    {
        _userManagerMock.Setup(u => u.FindByNameAsync("newuser"))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(u => u.SetAuthenticationTokenAsync(
            It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Regular" });

        var (response, refreshToken) = await _authService.RegisterAsync(new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@test.com",
            Password = "Test123!",
            FirstName = "New",
            LastName = "User"
        });

        Assert.NotNull(response.Access);
        Assert.NotNull(refreshToken);
        Assert.Equal("User Created Successfully.", response.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenInvalidCredentials_ThrowsException()
    {
        _userManagerMock.Setup(u => u.FindByNameAsync("wronguser"))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _authService.LoginAsync(new LoginRequest
            {
                Username = "wronguser",
                Password = "wrongpassword"
            }));
    }

    [Fact]
    public async Task LoginAsync_WhenValidCredentials_ReturnsAuthResponse()
    {
        var user = new ApplicationUser { UserName = "admin", Email = "admin@test.com", FirstName = "Admin", LastName = "User" };

        _userManagerMock.Setup(u => u.FindByNameAsync("admin"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(u => u.CheckPasswordAsync(user, "Admin123!"))
            .ReturnsAsync(true);

        _userManagerMock.Setup(u => u.SetAuthenticationTokenAsync(
            It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "Admin" });

        var (response, refreshToken) = await _authService.LoginAsync(new LoginRequest
        {
            Username = "admin",
            Password = "Admin123!"
        });

        Assert.NotNull(response.Access);
        Assert.NotNull(refreshToken);
        Assert.Equal("Login successful", response.Message);
    }

    [Fact]
    public async Task UpdatePasswordAsync_WhenUserNotFound_ThrowsException()
    {
        _userManagerMock.Setup(u => u.FindByIdAsync("invalid-id"))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<Exception>(() =>
            _authService.UpdatePasswordAsync("invalid-id", "oldpass", "newpass"));
    }
}