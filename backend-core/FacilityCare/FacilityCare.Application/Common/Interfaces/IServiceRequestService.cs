using FacilityCare.Application.DTOs.ServiceRequests;

namespace FacilityCare.Application.Common.Interfaces;

public interface IServiceRequestService
{
    Task<IList<ServiceRequestDto>> GetAllAsync(string userId, bool isAdmin, int? buildingId = null, int? serviceTypeId = null, string? priority = null, string? status = null, string? ordering = null);
    Task<ServiceRequestDto> GetByIdAsync(int id, string userId, bool isAdmin);
    Task<ServiceRequestDto> CreateAsync(CreateServiceRequestRequest request, string userId);
    Task DeleteAsync(int id, string userId, bool isAdmin);
    Task<ServiceRequestDto> UpdateStatusAsync(int id, UpdateServiceRequestStatusRequest request, string userId, bool isAdmin);
    Task<UserHomeDataResponse> GetUserHomeDataAsync(string userId);
}