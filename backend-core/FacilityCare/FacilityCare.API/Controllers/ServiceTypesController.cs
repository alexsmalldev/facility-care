using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.ServiceTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceTypesController : ControllerBase
{
    private readonly IServiceTypeService _serviceTypeService;

    public ServiceTypesController(IServiceTypeService serviceTypeService)
    {
        _serviceTypeService = serviceTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        try
        {
            var isAdmin = User.IsInRole("Admin");
            var serviceTypes = await _serviceTypeService.GetAllAsync(isAdmin, search);
            return Ok(serviceTypes);
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
            var serviceType = await _serviceTypeService.GetByIdAsync(id);
            return Ok(serviceType);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] CreateServiceTypeRequest request, IFormFile? serviceIcon)
    {
        try
        {
            var serviceType = await _serviceTypeService.CreateAsync(request, serviceIcon);
            return CreatedAtAction(nameof(GetById), new { id = serviceType.Id }, serviceType);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateServiceTypeRequest request, IFormFile? serviceIcon)
    {
        try
        {
            var serviceType = await _serviceTypeService.UpdateAsync(id, request, serviceIcon);
            return Ok(serviceType);
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
            await _serviceTypeService.DeleteAsync(id);
            return Ok(new { message = "Service type deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}