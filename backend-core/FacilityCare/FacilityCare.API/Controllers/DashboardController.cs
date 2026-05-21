using FacilityCare.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("requests-over-time")]
    public async Task<IActionResult> GetRequestsOverTime([FromQuery] string timeframe = "3months")
    {
        try
        {
            var result = await _dashboardService.GetRequestsOverTimeAsync(timeframe);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("todays-updates")]
    public async Task<IActionResult> GetTodaysUpdates()
    {
        try
        {
            var result = await _dashboardService.GetTodaysUpdatesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("action-required")]
    public async Task<IActionResult> GetActionRequired()
    {
        try
        {
            var result = await _dashboardService.GetActionRequiredAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("requests-by-building")]
    public async Task<IActionResult> GetRequestsByBuilding()
    {
        try
        {
            var result = await _dashboardService.GetRequestsByBuildingAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("requests-by-service-type")]
    public async Task<IActionResult> GetRequestsByServiceType()
    {
        try
        {
            var result = await _dashboardService.GetRequestsByServiceTypeAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("general-stats")]
    public async Task<IActionResult> GetGeneralStats()
    {
        try
        {
            var result = await _dashboardService.GetGeneralStatsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}