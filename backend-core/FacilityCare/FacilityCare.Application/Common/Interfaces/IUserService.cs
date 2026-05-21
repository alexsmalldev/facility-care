using FacilityCare.Application.DTOs.Users;

namespace FacilityCare.Application.Common.Interfaces;

public interface IUserService
{
    Task<IList<UserDto>> GetAllAsync(string? query);
    Task<UserDto> GetByIdAsync(string userId);
    Task<UserDto> UpdateAsync(string userId, UpdateUserRequest request);
    Task DeleteManyAsync(List<string> userIds);
    Task AssignBuildingsAsync(string userId, AssignBuildingsRequest request);
    Task<IList<UserDto>> GetAdminsAsync();
    Task<IList<UserDto>> GetUsersNotAssignedToBuildingAsync(int buildingId);
}