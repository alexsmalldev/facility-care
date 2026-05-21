using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FacilityCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] string? query)
    {
        try
        {
            var users = await _userService.GetAllAsync(query);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userService.GetByIdAsync(userId);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userService.UpdateAsync(userId, request);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("bulk-delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkDelete([FromBody] List<string> userIds)
    {
        try
        {
            if (!userIds.Any())
                return BadRequest(new { error = "No user IDs provided." });

            await _userService.DeleteManyAsync(userIds);
            return Ok(new { message = $"{userIds.Count} user(s) deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/assign-buildings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignBuildings(string id, [FromBody] AssignBuildingsRequest request)
    {
        try
        {
            if (!request.BuildingIds.Any())
                return BadRequest(new { error = "No building IDs provided." });

            await _userService.AssignBuildingsAsync(id, request);
            return Ok(new { message = "Buildings assigned to user successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("admins")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdmins()
    {
        try
        {
            var admins = await _userService.GetAdminsAsync();
            return Ok(admins);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("not-assigned-to-building")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsersNotAssignedToBuilding([FromQuery] int buildingId)
    {
        try
        {
            if (buildingId == 0)
                return BadRequest(new { error = "Building ID is required." });

            var users = await _userService.GetUsersNotAssignedToBuildingAsync(buildingId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("claims")]
    public IActionResult GetClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }
}