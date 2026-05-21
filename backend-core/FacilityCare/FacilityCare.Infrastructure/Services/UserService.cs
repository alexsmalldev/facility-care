using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.Users;
using FacilityCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FacilityCare.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public UserService(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IList<UserDto>> GetAllAsync(string? query)
    {
        var users = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(query))
            users = users.Where(u =>
                u.UserName!.Contains(query) ||
                u.Email!.Contains(query) ||
                u.FirstName.Contains(query) ||
                u.LastName.Contains(query));

        var userList = await users.ToListAsync();
        var result = new List<UserDto>();

        foreach (var user in userList)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(MapToDto(user, roles));
        }

        return result;
    }

    public async Task<UserDto> GetByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    public async Task<UserDto> UpdateAsync(string userId, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        if (!string.IsNullOrEmpty(request.Username)) user.UserName = request.Username;
        if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
        if (!string.IsNullOrEmpty(request.FirstName)) user.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName)) user.LastName = request.LastName;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles);
    }

    public async Task DeleteManyAsync(List<string> userIds)
    {
        foreach (var userId in userIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) continue;

            await _context.BuildingUsers
                .Where(bu => bu.UserId == userId)
                .ExecuteDeleteAsync();

            await _userManager.DeleteAsync(user);
        }
    }

    public async Task AssignBuildingsAsync(string userId, AssignBuildingsRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        var buildings = await _context.Buildings
            .Where(b => request.BuildingIds.Contains(b.Id))
            .ToListAsync();

        if (buildings.Count != request.BuildingIds.Count)
            throw new Exception("One or more buildings do not exist");

        var existingAssignments = _context.BuildingUsers.Where(bu => bu.UserId == userId);
        _context.BuildingUsers.RemoveRange(existingAssignments);

        foreach (var building in buildings)
        {
            _context.BuildingUsers.Add(new Domain.Entities.BuildingUser
            {
                BuildingId = building.Id,
                UserId = userId
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IList<UserDto>> GetAdminsAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var result = new List<UserDto>();

        foreach (var user in admins)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(MapToDto(user, roles));
        }

        return result;
    }

    public async Task<IList<UserDto>> GetUsersNotAssignedToBuildingAsync(int buildingId)
    {
        var building = await _context.Buildings.FindAsync(buildingId);
        if (building == null)
            throw new Exception("Building not found");

        var assignedUserIds = await _context.BuildingUsers
            .Where(bu => bu.BuildingId == buildingId)
            .Select(bu => bu.UserId)
            .ToListAsync();

        var allUsers = await _userManager.Users.ToListAsync();
        var unassignedUsers = allUsers.Where(u => !assignedUserIds.Contains(u.Id)).ToList();

        var result = new List<UserDto>();
        foreach (var user in unassignedUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin"))
                result.Add(MapToDto(user, roles));
        }

        return result;
    }

    private static UserDto MapToDto(ApplicationUser user, IList<string> roles) => new()
    {
        Id = user.Id,
        Username = user.UserName!,
        Email = user.Email!,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Roles = roles
    };
}