using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.Buildings;
using FacilityCare.Application.DTOs.Users;
using FacilityCare.Domain.Entities;
using FacilityCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FacilityCare.Infrastructure.Services;

public class BuildingService : IBuildingService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BuildingService(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IList<BuildingDto>> GetAllAsync(string userId, bool isAdmin)
    {
        var query = _context.Buildings.AsQueryable();

        if (!isAdmin)
            query = query.Where(b => b.BuildingUsers.Any(bu => bu.UserId == userId));

        var buildings = await query.ToListAsync();
        return buildings.Select(MapToDto).ToList();
    }

    public async Task<BuildingDto> GetByIdAsync(int id, string userId, bool isAdmin)
    {
        var building = await _context.Buildings
            .Include(b => b.BuildingUsers)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (building == null)
            throw new Exception("Building not found");

        if (!isAdmin && !building.BuildingUsers.Any(bu => bu.UserId == userId))
            throw new Exception("Access denied");

        return MapToDto(building);
    }

    public async Task<BuildingDto> CreateAsync(CreateBuildingRequest request)
    {
        var building = new Building
        {
            Name = request.Name,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            Postcode = request.Postcode,
            Country = request.Country,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        _context.Buildings.Add(building);
        await _context.SaveChangesAsync();
        return MapToDto(building);
    }

    public async Task<BuildingDto> UpdateAsync(int id, UpdateBuildingRequest request)
    {
        var building = await _context.Buildings.FindAsync(id);
        if (building == null)
            throw new Exception("Building not found");

        if (request.Name != null) building.Name = request.Name;
        if (request.AddressLine1 != null) building.AddressLine1 = request.AddressLine1;
        if (request.AddressLine2 != null) building.AddressLine2 = request.AddressLine2;
        if (request.City != null) building.City = request.City;
        if (request.Postcode != null) building.Postcode = request.Postcode;
        if (request.Country != null) building.Country = request.Country;
        if (request.Latitude != null) building.Latitude = request.Latitude.Value;
        if (request.Longitude != null) building.Longitude = request.Longitude.Value;

        await _context.SaveChangesAsync();
        return MapToDto(building);
    }

    public async Task DeleteAsync(int id)
    {
        var building = await _context.Buildings.FindAsync(id);
        if (building == null)
            throw new Exception("Building not found");

        _context.Buildings.Remove(building);
        await _context.SaveChangesAsync();
    }

    public async Task<IList<BuildingDto>> GetRegistrationListAsync()
    {
        var buildings = await _context.Buildings.ToListAsync();
        return buildings.Select(MapToDto).ToList();
    }

    public async Task<IList<UserDto>> GetBuildingUsersAsync(int id)
    {
        var building = await _context.Buildings
            .Include(b => b.BuildingUsers)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (building == null)
            throw new Exception("Building not found");

        var result = new List<UserDto>();
        foreach (var bu in building.BuildingUsers)
        {
            var user = await _userManager.FindByIdAsync(bu.UserId);
            if (user == null) continue;
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles
            });
        }

        return result;
    }

    public async Task UpdateBuildingUsersAsync(int id, UpdateBuildingUsersRequest request)
    {
        var building = await _context.Buildings
            .Include(b => b.BuildingUsers)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (building == null)
            throw new Exception("Building not found");

        foreach (var userId in request.UserIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception($"User {userId} not found");
        }

        _context.BuildingUsers.RemoveRange(building.BuildingUsers);

        foreach (var userId in request.UserIds)
        {
            _context.BuildingUsers.Add(new BuildingUser
            {
                BuildingId = id,
                UserId = userId
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IList<UserDto>> GetAvailableUsersAsync(int id)
    {
        var building = await _context.Buildings
            .Include(b => b.BuildingUsers)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (building == null)
            throw new Exception("Building not found");

        var assignedUserIds = building.BuildingUsers.Select(bu => bu.UserId).ToList();
        var allUsers = await _userManager.Users.ToListAsync();
        var availableUsers = allUsers.Where(u => !assignedUserIds.Contains(u.Id)).ToList();

        var result = new List<UserDto>();
        foreach (var user in availableUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin"))
                result.Add(new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName!,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles
                });
        }

        return result;
    }

    private static BuildingDto MapToDto(Building building) => new()
    {
        Id = building.Id,
        Name = building.Name,
        AddressLine1 = building.AddressLine1,
        AddressLine2 = building.AddressLine2,
        City = building.City,
        Postcode = building.Postcode,
        Country = building.Country,
        Latitude = building.Latitude,
        Longitude = building.Longitude
    };
}