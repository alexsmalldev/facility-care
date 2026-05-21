using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.Buildings;
using FacilityCare.Application.DTOs.ServiceRequests;
using FacilityCare.Application.DTOs.ServiceTypes;
using FacilityCare.Domain.Entities;
using FacilityCare.Domain.Enums;
using FacilityCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FacilityCare.Infrastructure.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ServiceRequestService(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IList<ServiceRequestDto>> GetAllAsync(string userId, bool isAdmin, int? buildingId = null, int? serviceTypeId = null, string? priority = null, string? status = null, string? ordering = null)
    {
        var query = _context.ServiceRequests
            .Include(sr => sr.ServiceType)
            .Include(sr => sr.Building)
            .AsQueryable();

        if (!isAdmin)
            query = query.Where(sr => sr.CreatedById == userId);

        if (buildingId.HasValue)
            query = query.Where(sr => sr.BuildingId == buildingId.Value);

        if (serviceTypeId.HasValue)
            query = query.Where(sr => sr.ServiceTypeId == serviceTypeId.Value);

        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<ServiceRequestPriority>(priority, true, out var priorityEnum))
            query = query.Where(sr => sr.Priority == priorityEnum);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ServiceRequestStatus>(status, true, out var statusEnum))
            query = query.Where(sr => sr.Status == statusEnum);

        query = ordering switch
        {
            "id" => query.OrderBy(sr => sr.Id),
            "-id" => query.OrderByDescending(sr => sr.Id),
            "createdDate" => query.OrderBy(sr => sr.CreatedDate),
            "-createdDate" => query.OrderByDescending(sr => sr.CreatedDate),
            "serviceLevelAgreementDate" => query.OrderBy(sr => sr.ServiceLevelAgreementDate),
            "-serviceLevelAgreementDate" => query.OrderByDescending(sr => sr.ServiceLevelAgreementDate),
            "priority" => query.OrderBy(sr => sr.Priority),
            "-priority" => query.OrderByDescending(sr => sr.Priority),
            "status" => query.OrderBy(sr => sr.Status),
            "-status" => query.OrderByDescending(sr => sr.Status),
            _ => isAdmin ? query.OrderBy(sr => sr.ServiceLevelAgreementDate) : query.OrderByDescending(sr => sr.CreatedDate)
        };

        var requests = await query.ToListAsync();
        var result = new List<ServiceRequestDto>();
        foreach (var sr in requests)
            result.Add(await MapToDto(sr));
        return result;
    }

    public async Task<ServiceRequestDto> GetByIdAsync(int id, string userId, bool isAdmin)
    {
        var request = await _context.ServiceRequests
            .Include(sr => sr.ServiceType)
            .Include(sr => sr.Building)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (request == null)
            throw new Exception("Service request not found");

        if (!isAdmin && request.CreatedById != userId)
            throw new UnauthorizedAccessException("You do not have permission to access this service request");

        return await MapToDto(request);
    }

    public async Task<ServiceRequestDto> CreateAsync(CreateServiceRequestRequest request, string userId)
    {
        var serviceType = await _context.ServiceTypes.FindAsync(request.ServiceTypeId);
        if (serviceType == null)
            throw new Exception("Service type not found");

        if (!serviceType.IsActive)
            throw new Exception("The selected service is no longer available");

        var building = await _context.Buildings.FindAsync(request.BuildingId);
        if (building == null)
            throw new Exception("Building not found");

        var slaDate = request.Priority switch
        {
            "High" => DateTime.UtcNow.AddDays(1),
            "Medium" => DateTime.UtcNow.AddDays(3),
            _ => DateTime.UtcNow.AddDays(5)
        };

        var serviceRequest = new ServiceRequest
        {
            CustomerNotes = request.CustomerNotes,
            Priority = Enum.Parse<ServiceRequestPriority>(request.Priority),
            Status = ServiceRequestStatus.Open,
            ServiceTypeId = request.ServiceTypeId,
            BuildingId = request.BuildingId,
            CreatedById = userId,
            ServiceLevelAgreementDate = slaDate
        };

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();

        _context.Updates.Add(new Update
        {
            Title = "Request Created",
            Message = $"Request {serviceRequest.Id} created",
            CreatedById = userId,
            ServiceRequestId = serviceRequest.Id,
            Type = UpdateType.Event
        });

        await _context.SaveChangesAsync();

        serviceRequest.ServiceType = serviceType;
        serviceRequest.Building = building;

        return await MapToDto(serviceRequest);
    }

    public async Task DeleteAsync(int id, string userId, bool isAdmin)
    {
        var request = await _context.ServiceRequests.FindAsync(id);
        if (request == null)
            throw new Exception("Service request not found");

        if (!isAdmin && request.CreatedById != userId)
            throw new UnauthorizedAccessException("You do not have permission to delete this service request");

        _context.ServiceRequests.Remove(request);
        await _context.SaveChangesAsync();
    }

    public async Task<ServiceRequestDto> UpdateStatusAsync(int id, UpdateServiceRequestStatusRequest request, string userId, bool isAdmin)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.ServiceType)
            .Include(sr => sr.Building)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null)
            throw new Exception("Service request not found");

        if (!isAdmin)
            throw new UnauthorizedAccessException("Only admins can update service request status");

        if (!Enum.TryParse<ServiceRequestStatus>(request.Status, out var newStatus))
            throw new Exception("Invalid status value");

        serviceRequest.Status = newStatus;
        serviceRequest.UpdatedDate = DateTime.UtcNow;

        _context.Updates.Add(new Update
        {
            Title = $"Request {serviceRequest.Id} Status Update",
            Message = $"Request {serviceRequest.Id} has been moved to {newStatus}",
            CreatedById = userId,
            AssociatedToId = serviceRequest.CreatedById,
            ServiceRequestId = serviceRequest.Id,
            Type = UpdateType.Event
        });

        if (!string.IsNullOrEmpty(request.Comment))
        {
            _context.Updates.Add(new Update
            {
                Title = $"A Comment has been added to Request {serviceRequest.Id}",
                Message = request.Comment,
                CreatedById = userId,
                AssociatedToId = serviceRequest.CreatedById,
                ServiceRequestId = serviceRequest.Id,
                Type = UpdateType.Message
            });
        }

        await _context.SaveChangesAsync();
        return await MapToDto(serviceRequest);
    }

    public async Task<UserHomeDataResponse> GetUserHomeDataAsync(string userId)
    {
        var userRequests = await _context.ServiceRequests
            .Include(sr => sr.ServiceType)
            .Include(sr => sr.Building)
            .Where(sr => sr.CreatedById == userId)
            .OrderByDescending(sr => sr.CreatedDate)
            .Take(10)
            .ToListAsync();

        var recentServiceTypeIds = userRequests
            .Select(sr => sr.ServiceTypeId)
            .Distinct()
            .Take(10)
            .ToList();

        var recentServiceTypes = await _context.ServiceTypes
            .Where(st => recentServiceTypeIds.Contains(st.Id))
            .ToListAsync();

        var result = new List<ServiceRequestDto>();
        foreach (var sr in userRequests)
            result.Add(await MapToDto(sr));

        return new UserHomeDataResponse
        {
            RecentRequests = result,
            RecentServiceTypes = recentServiceTypes.Select(st => new ServiceTypeDto
            {
                Id = st.Id,
                Name = st.Name,
                Description = st.Description,
                ServiceIcon = st.ServiceIcon,
                IsActive = st.IsActive
            }).ToList()
        };
    }

    private async Task<ServiceRequestDto> MapToDto(ServiceRequest sr)
    {
        var createdBy = await _userManager.FindByIdAsync(sr.CreatedById);
        return new ServiceRequestDto
        {
            Id = sr.Id,
            CustomerNotes = sr.CustomerNotes,
            Status = sr.Status.ToString(),
            Priority = sr.Priority.ToString(),
            CreatedDate = sr.CreatedDate,
            UpdatedDate = sr.UpdatedDate,
            ServiceLevelAgreementDate = sr.ServiceLevelAgreementDate,
            CreatedById = sr.CreatedById,
            CreatedByFirstName = createdBy?.FirstName ?? string.Empty,
            CreatedByLastName = createdBy?.LastName ?? string.Empty,
            ServiceType = new ServiceTypeDto
            {
                Id = sr.ServiceType.Id,
                Name = sr.ServiceType.Name,
                Description = sr.ServiceType.Description,
                ServiceIcon = sr.ServiceType.ServiceIcon,
                IsActive = sr.ServiceType.IsActive
            },
            Building = new BuildingDto
            {
                Id = sr.Building.Id,
                Name = sr.Building.Name,
                AddressLine1 = sr.Building.AddressLine1,
                AddressLine2 = sr.Building.AddressLine2,
                City = sr.Building.City,
                Postcode = sr.Building.Postcode,
                Country = sr.Building.Country,
                Latitude = sr.Building.Latitude,
                Longitude = sr.Building.Longitude
            }
        };
    }
}