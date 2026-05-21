using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.Updates;
using FacilityCare.Domain.Entities;
using FacilityCare.Domain.Enums;
using FacilityCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FacilityCare.Infrastructure.Services;

public class UpdateService : IUpdateService
{
    private readonly AppDbContext _context;

    public UpdateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IList<UpdateDto>> GetAllAsync(string userId, bool isAdmin)
    {
        var query = _context.Updates.AsQueryable();

        if (!isAdmin)
            query = query.Where(u => u.AssociatedToId == userId);

        var updates = await query.OrderByDescending(u => u.CreatedDate).ToListAsync();
        return updates.Select(MapToDto).ToList();
    }

    public async Task<IList<UpdateDto>> GetByServiceRequestAsync(int serviceRequestId)
    {
        var updates = await _context.Updates
            .Where(u => u.ServiceRequestId == serviceRequestId)
            .OrderByDescending(u => u.CreatedDate)
            .ToListAsync();

        return updates.Select(MapToDto).ToList();
    }

    public async Task<UpdateDto> CreateAsync(CreateUpdateRequest request, string userId, bool isAdmin)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.ServiceType)
            .Include(sr => sr.Building)
            .FirstOrDefaultAsync(sr => sr.Id == request.ServiceRequestId);

        if (serviceRequest == null)
            throw new Exception("Service request not found");

        var associatedToId = isAdmin ? serviceRequest.CreatedById : null;

        var update = new Update
        {
            Title = $"A Comment has been added to Request {serviceRequest.Id}",
            Message = request.Message,
            CreatedById = userId,
            AssociatedToId = associatedToId,
            ServiceRequestId = serviceRequest.Id,
            Type = UpdateType.Message
        };

        _context.Updates.Add(update);
        await _context.SaveChangesAsync();

        return MapToDto(update);
    }

    public async Task<IList<UpdateDto>> GetNotificationsAsync(string userId)
    {
        var updates = await _context.Updates
            .Where(u => u.AssociatedToId == userId && !u.IsRead && u.CreatedById != userId)
            .OrderBy(u => u.IsRead)
            .ThenByDescending(u => u.CreatedDate)
            .ToListAsync();

        return updates.Select(MapToDto).ToList();
    }

    public async Task MarkAllReadAsync(string userId)
    {
        var updates = await _context.Updates
            .Where(u => u.AssociatedToId == userId && !u.IsRead)
            .ToListAsync();

        foreach (var update in updates)
            update.IsRead = true;

        await _context.SaveChangesAsync();
    }

    public async Task MarkReadAsync(int id, string userId)
    {
        var update = await _context.Updates.FindAsync(id);
        if (update == null)
            throw new Exception("Update not found");

        if (update.AssociatedToId != userId)
            throw new UnauthorizedAccessException("You do not have permission to mark this update as read");

        update.IsRead = true;
        await _context.SaveChangesAsync();
    }

    private static UpdateDto MapToDto(Update update) => new()
    {
        Id = update.Id,
        Title = update.Title,
        Message = update.Message,
        CreatedDate = update.CreatedDate,
        CreatedById = update.CreatedById,
        AssociatedToId = update.AssociatedToId,
        ServiceRequestId = update.ServiceRequestId,
        Type = update.Type.ToString(),
        IsRead = update.IsRead
    };
}