using FacilityCare.Application.DTOs.Buildings;
using FacilityCare.Application.DTOs.Users;

namespace FacilityCare.Application.Common.Interfaces;

public interface IBuildingService
{
    Task<IList<BuildingDto>> GetAllAsync(string userId, bool isAdmin);
    Task<BuildingDto> GetByIdAsync(int id, string userId, bool isAdmin);
    Task<BuildingDto> CreateAsync(CreateBuildingRequest request);
    Task<BuildingDto> UpdateAsync(int id, UpdateBuildingRequest request);
    Task DeleteAsync(int id);
    Task<IList<BuildingDto>> GetRegistrationListAsync();
    Task<IList<UserDto>> GetBuildingUsersAsync(int id);
    Task UpdateBuildingUsersAsync(int id, UpdateBuildingUsersRequest request);
    Task<IList<UserDto>> GetAvailableUsersAsync(int id);
}