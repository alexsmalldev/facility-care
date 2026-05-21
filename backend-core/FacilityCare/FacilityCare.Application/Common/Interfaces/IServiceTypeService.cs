using FacilityCare.Application.DTOs.ServiceTypes;
using Microsoft.AspNetCore.Http;

namespace FacilityCare.Application.Common.Interfaces;

public interface IServiceTypeService
{
    Task<IList<ServiceTypeDto>> GetAllAsync(bool isAdmin, string? search);
    Task<ServiceTypeDto> GetByIdAsync(int id);
    Task<ServiceTypeDto> CreateAsync(CreateServiceTypeRequest request, IFormFile? icon);
    Task<ServiceTypeDto> UpdateAsync(int id, UpdateServiceTypeRequest request, IFormFile? icon);
    Task DeleteAsync(int id);
}