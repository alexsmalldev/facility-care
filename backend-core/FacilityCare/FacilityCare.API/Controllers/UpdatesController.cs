using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.Updates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FacilityCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UpdatesController : ControllerBase
{
    private readonly IUpdateService _updateService;

    public UpdatesController(IUpdateService updateService)
    {
        _updateService = updateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");
            var updates = await _updateService.GetAllAsync(userId, isAdmin);
            return Ok(updates);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("service-request")]
    public async Task<IActionResult> GetByServiceRequest([FromQuery] int serviceRequestId)
    {
        try
        {
            if (serviceRequestId == 0)
                return BadRequest(new { error = "service_request_id is required" });

            var updates = await _updateService.GetByServiceRequestAsync(serviceRequestId);
            return Ok(updates);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUpdateRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");
            var update = await _updateService.CreateAsync(request, userId, isAdmin);
            return CreatedAtAction(nameof(GetAll), update);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var notifications = await _updateService.GetNotificationsAsync(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _updateService.MarkAllReadAsync(userId);
            return Ok(new { message = "All updates marked as read" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/mark-read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _updateService.MarkReadAsync(id, userId);
            return Ok(new { message = "Update marked as read" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}