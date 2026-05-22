using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.ServiceTypes;
using FacilityCare.Domain.Entities;
using FacilityCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FacilityCare.Infrastructure.Services;

public class ServiceTypeService : IServiceTypeService
{
    private readonly AppDbContext _context;
    private readonly IFileService _fileService;

    public ServiceTypeService(AppDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<IList<ServiceTypeDto>> GetAllAsync(bool isAdmin, string? search)
    {
        var query = _context.ServiceTypes.AsQueryable();

        if (!isAdmin)
            query = query.Where(st => st.IsActive);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(st =>
                st.Name.Contains(search) ||
                st.Description.Contains(search));

        var serviceTypes = await query.OrderBy(st => st.Name).ToListAsync();
        return serviceTypes.Select(MapToDto).ToList();
    }

    public async Task<ServiceTypeDto> GetByIdAsync(int id)
    {
        var serviceType = await _context.ServiceTypes.FindAsync(id);
        if (serviceType == null)
            throw new Exception("Service type not found");

        return MapToDto(serviceType);
    }

    public async Task<ServiceTypeDto> CreateAsync(CreateServiceTypeRequest request, IFormFile? icon)
    {
        if (request.IsPaid && (request.Price == null || request.Price <= 0))
            throw new Exception("A valid price must be provided for paid services.");

        var serviceType = new ServiceType
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            IsPaid = request.IsPaid,
            Price = request.IsPaid ? request.Price : null
        };

        if (icon != null)
            serviceType.ServiceIcon = await _fileService.UploadFileAsync(icon, "service-icons");

        _context.ServiceTypes.Add(serviceType);
        await _context.SaveChangesAsync();
        return MapToDto(serviceType);
    }

    public async Task<ServiceTypeDto> UpdateAsync(int id, UpdateServiceTypeRequest request, IFormFile? icon)
    {
        var serviceType = await _context.ServiceTypes.FindAsync(id);
        if (serviceType == null)
            throw new Exception("Service type not found");

        if (request.Name != null) serviceType.Name = request.Name;
        if (request.Description != null) serviceType.Description = request.Description;
        if (request.IsActive != null) serviceType.IsActive = request.IsActive.Value;

        if (icon != null)
        {
            if (serviceType.ServiceIcon != null)
                await _fileService.DeleteFileAsync(serviceType.ServiceIcon);

            serviceType.ServiceIcon = await _fileService.UploadFileAsync(icon, "service-icons");
        }

        await _context.SaveChangesAsync();
        return MapToDto(serviceType);
    }

    public async Task DeleteAsync(int id)
    {
        var serviceType = await _context.ServiceTypes.FindAsync(id);
        if (serviceType == null)
            throw new Exception("Service type not found");

        if (serviceType.ServiceIcon != null)
            await _fileService.DeleteFileAsync(serviceType.ServiceIcon);

        _context.ServiceTypes.Remove(serviceType);
        await _context.SaveChangesAsync();
    }

    private static ServiceTypeDto MapToDto(ServiceType serviceType) => new()
    {
        Id = serviceType.Id,
        Name = serviceType.Name,
        Description = serviceType.Description,
        ServiceIcon = serviceType.ServiceIcon,
        IsActive = serviceType.IsActive,
        IsPaid = serviceType.IsPaid,
        Price = serviceType.Price
    };
}