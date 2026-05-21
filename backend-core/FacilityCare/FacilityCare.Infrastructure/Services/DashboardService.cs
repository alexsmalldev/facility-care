using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.Buildings;
using FacilityCare.Application.DTOs.Dashboard;
using FacilityCare.Application.DTOs.ServiceRequests;
using FacilityCare.Application.DTOs.ServiceTypes;
using FacilityCare.Application.DTOs.Updates;
using FacilityCare.Domain.Enums;
using FacilityCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FacilityCare.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardRequestsOverTimeResponse> GetRequestsOverTimeAsync(string timeframe)
    {
        var startDate = timeframe switch
        {
            "7days" => DateTime.UtcNow.AddDays(-7),
            "30days" => DateTime.UtcNow.AddDays(-30),
            _ => DateTime.UtcNow.AddDays(-90)
        };

        var requestsOverTime = await _context.ServiceRequests
            .Where(sr => sr.CreatedDate >= startDate)
            .GroupBy(sr => sr.CreatedDate.Date)
            .Select(g => new RequestsOverTimeDto
            {
                Day = g.Key,
                Count = g.Count()
            })
            .OrderBy(r => r.Day)
            .ToListAsync();

        return new DashboardRequestsOverTimeResponse { RequestsOverTime = requestsOverTime };
    }

    public async Task<DashboardTodaysUpdatesResponse> GetTodaysUpdatesAsync()
    {
        var today = DateTime.UtcNow.Date;

        var updates = await _context.Updates
            .Where(u => u.CreatedDate.Date == today && u.Type == UpdateType.Message)
            .OrderByDescending(u => u.CreatedDate)
            .ToListAsync();

        return new DashboardTodaysUpdatesResponse
        {
            UpdatesToday = updates.Select(u => new UpdateDto
            {
                Id = u.Id,
                Title = u.Title,
                Message = u.Message,
                CreatedDate = u.CreatedDate,
                CreatedById = u.CreatedById,
                AssociatedToId = u.AssociatedToId,
                ServiceRequestId = u.ServiceRequestId,
                Type = u.Type.ToString(),
                IsRead = u.IsRead
            }).ToList()
        };
    }

    public async Task<DashboardActionRequiredResponse> GetActionRequiredAsync()
    {
        var threeDaysAhead = DateTime.UtcNow.AddDays(3);

        var urgentRequests = await _context.ServiceRequests
            .Include(sr => sr.ServiceType)
            .Include(sr => sr.Building)
            .Where(sr =>
                sr.ServiceLevelAgreementDate <= threeDaysAhead &&
                (sr.Status == ServiceRequestStatus.Open || sr.Status == ServiceRequestStatus.InProgress))
            .OrderBy(sr => sr.ServiceLevelAgreementDate)
            .ToListAsync();

        return new DashboardActionRequiredResponse
        {
            ActionsRequired = urgentRequests.Select(sr => new ServiceRequestDto
            {
                Id = sr.Id,
                CustomerNotes = sr.CustomerNotes,
                Status = sr.Status.ToString(),
                Priority = sr.Priority.ToString(),
                CreatedDate = sr.CreatedDate,
                UpdatedDate = sr.UpdatedDate,
                ServiceLevelAgreementDate = sr.ServiceLevelAgreementDate,
                CreatedById = sr.CreatedById,
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
            }).ToList()
        };
    }

    public async Task<DashboardRequestsByBuildingResponse> GetRequestsByBuildingAsync()
    {
        var requestsByBuilding = await _context.ServiceRequests
            .Include(sr => sr.Building)
            .GroupBy(sr => sr.Building.Name)
            .Select(g => new RequestsByBuildingDto
            {
                BuildingName = g.Key ?? "Unknown",
                Count = g.Count()
            })
            .OrderByDescending(r => r.Count)
            .ToListAsync();

        return new DashboardRequestsByBuildingResponse { RequestsByBuilding = requestsByBuilding };
    }

    public async Task<DashboardRequestsByServiceTypeResponse> GetRequestsByServiceTypeAsync()
    {
        var requestsByServiceType = await _context.ServiceRequests
            .Include(sr => sr.ServiceType)
            .GroupBy(sr => sr.ServiceType.Name)
            .Select(g => new RequestsByServiceTypeDto
            {
                ServiceTypeName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(r => r.Count)
            .ToListAsync();

        return new DashboardRequestsByServiceTypeResponse { RequestsByServiceType = requestsByServiceType };
    }

    public async Task<DashboardGeneralStatsResponse> GetGeneralStatsAsync()
    {
        var stats = new GeneralStatsDto
        {
            OpenRequests = await _context.ServiceRequests
                .CountAsync(sr => sr.Status == ServiceRequestStatus.Open),
            InProgressRequests = await _context.ServiceRequests
                .CountAsync(sr => sr.Status == ServiceRequestStatus.InProgress),
            CompletedRequests = await _context.ServiceRequests
                .CountAsync(sr => sr.Status == ServiceRequestStatus.Completed)
        };

        return new DashboardGeneralStatsResponse { Stats = stats };
    }
}