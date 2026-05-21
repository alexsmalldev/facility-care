using FacilityCare.Application.DTOs.Updates;

namespace FacilityCare.Application.Common.Interfaces;

public interface IUpdateService
{
    Task<IList<UpdateDto>> GetAllAsync(string userId, bool isAdmin);
    Task<IList<UpdateDto>> GetByServiceRequestAsync(int serviceRequestId);
    Task<UpdateDto> CreateAsync(CreateUpdateRequest request, string userId, bool isAdmin);
    Task<IList<UpdateDto>> GetNotificationsAsync(string userId);
    Task MarkAllReadAsync(string userId);
    Task MarkReadAsync(int id, string userId);
}