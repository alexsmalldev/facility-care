using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.Buildings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FacilityCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingsController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? query, [FromQuery] string? ordering)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");
            var buildings = await _buildingService.GetAllAsync(userId, isAdmin, query, ordering);
            return Ok(buildings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");
            var building = await _buildingService.GetByIdAsync(id, userId, isAdmin);
            return Ok(building);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBuildingRequest request)
    {
        try
        {
            var building = await _buildingService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = building.Id }, building);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBuildingRequest request)
    {
        try
        {
            var building = await _buildingService.UpdateAsync(id, request);
            return Ok(building);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _buildingService.DeleteAsync(id);
            return Ok(new { message = "Building deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("registration-list")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRegistrationList()
    {
        try
        {
            var buildings = await _buildingService.GetRegistrationListAsync();
            return Ok(buildings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetBuildingUsers(int id)
    {
        try
        {
            var users = await _buildingService.GetBuildingUsersAsync(id);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBuildingUsers(int id, [FromBody] UpdateBuildingUsersRequest request)
    {
        try
        {
            await _buildingService.UpdateBuildingUsersAsync(id, request);
            return Ok(new { message = "Building users updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/available-users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAvailableUsers(int id)
    {
        try
        {
            var users = await _buildingService.GetAvailableUsersAsync(id);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}