using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.Auth;
using FacilityCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FacilityCare.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<(AuthResponse response, string refreshToken)> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByNameAsync(request.Username);
        if (existingUser != null)
            throw new Exception("Username already exists");

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "Regular");

        var roles = await _userManager.GetRolesAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync(user);
        return (new AuthResponse
        {
            Access = GenerateAccessToken(user, roles),
            Message = "User Created Successfully."
        }, refreshToken);
    }

    public async Task<(AuthResponse response, string refreshToken)> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            throw new Exception("Invalid credentials");

        var roles = await _userManager.GetRolesAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync(user);
        return (new AuthResponse
        {
            Access = GenerateAccessToken(user, roles),
            Message = "Login successful"
        }, refreshToken);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var user = await FindUserByRefreshTokenAsync(refreshToken);
        if (user == null)
            throw new Exception("Invalid refresh token");

        await _userManager.RemoveAuthenticationTokenAsync(user, "FacilityCare", "RefreshToken");
    }

    public async Task<(AuthResponse response, string refreshToken)> RefreshTokenAsync(string refreshToken)
    {
        var user = await FindUserByRefreshTokenAsync(refreshToken);
        if (user == null)
            throw new Exception("Invalid refresh token");

        var roles = await _userManager.GetRolesAsync(user);
        var newRefreshToken = await GenerateRefreshTokenAsync(user);
        return (new AuthResponse
        {
            Access = GenerateAccessToken(user, roles),
            Message = "Token refreshed successfully."
        }, newRefreshToken);
    }

    public async Task UpdatePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    private string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("first_name", user.FirstName),
            new Claim("last_name", user.LastName)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshTokenAsync(ApplicationUser user)
    {
        var refreshToken = Guid.NewGuid().ToString();
        await _userManager.SetAuthenticationTokenAsync(user, "FacilityCare", "RefreshToken", refreshToken);
        return refreshToken;
    }

    private async Task<ApplicationUser?> FindUserByRefreshTokenAsync(string refreshToken)
    {
        var users = _userManager.Users.ToList();
        foreach (var user in users)
        {
            var token = await _userManager.GetAuthenticationTokenAsync(user, "FacilityCare", "RefreshToken");
            if (token == refreshToken)
                return user;
        }
        return null;
    }
}