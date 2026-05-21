using FacilityCare.Application.DTOs.Updates;
using FacilityCare.Domain.Entities;
using FacilityCare.Domain.Enums;
using FacilityCare.Infrastructure.Persistence;
using FacilityCare.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FacilityCare.Application.Tests.Services;

public class UpdateServiceTests
{
    private readonly AppDbContext _context;
    private readonly UpdateService _updateService;

    public UpdateServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _updateService = new UpdateService(_context);

        SeedData();
    }

    private void SeedData()
    {
        _context.ServiceTypes.Add(new ServiceType
        {
            Id = 1,
            Name = "Cleaning",
            Description = "Cleaning services",
            IsActive = true
        });

        _context.Buildings.Add(new Building
        {
            Id = 1,
            Name = "Test Building",
            AddressLine1 = "123 Test St",
            City = "Test City",
            Postcode = "12345",
            Latitude = 51.5074m,
            Longitude = -0.1278m
        });

        _context.ServiceRequests.Add(new ServiceRequest
        {
            Id = 1,
            Status = ServiceRequestStatus.Open,
            Priority = ServiceRequestPriority.Low,
            CreatedById = "regular-user-id",
            ServiceTypeId = 1,
            BuildingId = 1,
            ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(5)
        });

        _context.Updates.AddRange(
            new Update
            {
                Id = 1,
                Title = "Request Created",
                Message = "Request 1 created",
                CreatedById = "regular-user-id",
                AssociatedToId = null,
                ServiceRequestId = 1,
                Type = UpdateType.Event,
                IsRead = false
            },
            new Update
            {
                Id = 2,
                Title = "Status Update",
                Message = "Request moved to In Progress",
                CreatedById = "admin-id",
                AssociatedToId = "regular-user-id",
                ServiceRequestId = 1,
                Type = UpdateType.Event,
                IsRead = false
            },
            new Update
            {
                Id = 3,
                Title = "Comment",
                Message = "Working on it",
                CreatedById = "admin-id",
                AssociatedToId = "regular-user-id",
                ServiceRequestId = 1,
                Type = UpdateType.Message,
                IsRead = true
            }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_AsAdmin_ReturnsAllUpdates()
    {
        var result = await _updateService.GetAllAsync("admin-id", isAdmin: true);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_AsRegularUser_ReturnsOnlyAssociatedUpdates()
    {
        var result = await _updateService.GetAllAsync("regular-user-id", isAdmin: false);
        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.Equal("regular-user-id", u.AssociatedToId));
    }

    [Fact]
    public async Task GetByServiceRequestAsync_ReturnsUpdatesForRequest()
    {
        var result = await _updateService.GetByServiceRequestAsync(1);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetByServiceRequestAsync_WithInvalidId_ReturnsEmpty()
    {
        var result = await _updateService.GetByServiceRequestAsync(999);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_WhenServiceRequestNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _updateService.CreateAsync(new CreateUpdateRequest
            {
                Message = "Test comment",
                ServiceRequestId = 999
            }, "admin-id", isAdmin: true));
    }

    [Fact]
    public async Task CreateAsync_AsAdmin_SetsAssociatedToRequestCreator()
    {
        var result = await _updateService.CreateAsync(new CreateUpdateRequest
        {
            Message = "Admin comment",
            ServiceRequestId = 1
        }, "admin-id", isAdmin: true);

        Assert.Equal("regular-user-id", result.AssociatedToId);
        Assert.Equal("Message", result.Type);
    }

    [Fact]
    public async Task CreateAsync_AsRegularUser_SetsAssociatedToNull()
    {
        var result = await _updateService.CreateAsync(new CreateUpdateRequest
        {
            Message = "User comment",
            ServiceRequestId = 1
        }, "regular-user-id", isAdmin: false);

        Assert.Null(result.AssociatedToId);
    }

    [Fact]
    public async Task GetNotificationsAsync_ReturnsUnreadNotificationsForUser()
    {
        var result = await _updateService.GetNotificationsAsync("regular-user-id");
        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public async Task MarkAllReadAsync_MarksAllUserNotificationsAsRead()
    {
        await _updateService.MarkAllReadAsync("regular-user-id");

        var unread = await _context.Updates
            .Where(u => u.AssociatedToId == "regular-user-id" && !u.IsRead)
            .CountAsync();

        Assert.Equal(0, unread);
    }

    [Fact]
    public async Task MarkReadAsync_WhenUpdateNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _updateService.MarkReadAsync(999, "regular-user-id"));
    }

    [Fact]
    public async Task MarkReadAsync_WhenNotAssociatedToUser_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _updateService.MarkReadAsync(1, "another-user-id"));
    }

    [Fact]
    public async Task MarkReadAsync_WhenValid_MarksUpdateAsRead()
    {
        await _updateService.MarkReadAsync(2, "regular-user-id");

        var update = await _context.Updates.FindAsync(2);
        Assert.True(update!.IsRead);
    }
}