using FacilityCare.Application.DTOs.Auth;

namespace FacilityCare.Application.Common.Interfaces;

public interface IAuthService
{
    Task<(AuthResponse response, string refreshToken)> RegisterAsync(RegisterRequest request);
    Task<(AuthResponse response, string refreshToken)> LoginAsync(LoginRequest request);
    Task LogoutAsync(string refreshToken);
    Task<(AuthResponse response, string refreshToken)> RefreshTokenAsync(string refreshToken);
    Task UpdatePasswordAsync(string userId, string currentPassword, string newPassword);
}