using FacilityCare.Application.DTOs.ServiceRequests;
using FacilityCare.Domain.Entities;
using FacilityCare.Domain.Enums;
using FacilityCare.Infrastructure.Persistence;
using FacilityCare.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FacilityCare.Application.Tests.Services;

public class ServiceRequestServiceTests
{
    private readonly AppDbContext _context;
    private readonly ServiceRequestService _serviceRequestService;

    public ServiceRequestServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _serviceRequestService = new ServiceRequestService(_context);

        SeedData();
    }

    private void SeedData()
    {
        _context.ServiceTypes.AddRange(
            new ServiceType { Id = 1, Name = "Cleaning", Description = "Cleaning services", IsActive = true },
            new ServiceType { Id = 2, Name = "Inactive Service", Description = "Inactive", IsActive = false }
        );

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

        _context.ServiceRequests.AddRange(
            new ServiceRequest
            {
                Id = 1,
                CustomerNotes = "Test notes",
                Status = ServiceRequestStatus.Open,
                Priority = ServiceRequestPriority.Low,
                CreatedById = "regular-user-id",
                ServiceTypeId = 1,
                BuildingId = 1,
                ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(5)
            },
            new ServiceRequest
            {
                Id = 2,
                CustomerNotes = "Admin request",
                Status = ServiceRequestStatus.InProgress,
                Priority = ServiceRequestPriority.High,
                CreatedById = "another-user-id",
                ServiceTypeId = 1,
                BuildingId = 1,
                ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(1)
            }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_AsAdmin_ReturnsAllRequests()
    {
        var result = await _serviceRequestService.GetAllAsync("admin-id", isAdmin: true);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_AsRegularUser_ReturnsOnlyOwnRequests()
    {
        var result = await _serviceRequestService.GetAllAsync("regular-user-id", isAdmin: false);
        Assert.Single(result);
        Assert.Equal("regular-user-id", result[0].CreatedById);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _serviceRequestService.GetByIdAsync(999, "admin-id", isAdmin: true));
    }

    [Fact]
    public async Task GetByIdAsync_AsRegularUser_WhenNotOwner_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _serviceRequestService.GetByIdAsync(1, "another-user-id", isAdmin: false));
    }

    [Fact]
    public async Task GetByIdAsync_AsRegularUser_WhenOwner_ReturnsRequest()
    {
        var result = await _serviceRequestService.GetByIdAsync(1, "regular-user-id", isAdmin: false);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_AsAdmin_ReturnsAnyRequest()
    {
        var result = await _serviceRequestService.GetByIdAsync(2, "admin-id", isAdmin: true);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public async Task CreateAsync_WithInactiveServiceType_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _serviceRequestService.CreateAsync(new CreateServiceRequestRequest
            {
                ServiceTypeId = 2,
                BuildingId = 1,
                Priority = "Low"
            }, "regular-user-id"));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidBuilding_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _serviceRequestService.CreateAsync(new CreateServiceRequestRequest
            {
                ServiceTypeId = 1,
                BuildingId = 999,
                Priority = "Low"
            }, "regular-user-id"));
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesServiceRequestAndUpdate()
    {
        var result = await _serviceRequestService.CreateAsync(new CreateServiceRequestRequest
        {
            CustomerNotes = "New request",
            ServiceTypeId = 1,
            BuildingId = 1,
            Priority = "High"
        }, "regular-user-id");

        Assert.Equal("New request", result.CustomerNotes);
        Assert.Equal("Open", result.Status);
        Assert.Equal(3, await _context.ServiceRequests.CountAsync());
        Assert.True(await _context.Updates.AnyAsync(u => u.ServiceRequestId == result.Id));
    }

    [Fact]
    public async Task CreateAsync_WithHighPriority_SetsSlaToOneDay()
    {
        var result = await _serviceRequestService.CreateAsync(new CreateServiceRequestRequest
        {
            ServiceTypeId = 1,
            BuildingId = 1,
            Priority = "High"
        }, "regular-user-id");

        Assert.NotNull(result.ServiceLevelAgreementDate);
        var expectedSla = DateTime.UtcNow.AddDays(1);
        Assert.True(result.ServiceLevelAgreementDate!.Value <= expectedSla.AddMinutes(1));
    }

    [Fact]
    public async Task CreateAsync_WithMediumPriority_SetsSlaToThreeDays()
    {
        var result = await _serviceRequestService.CreateAsync(new CreateServiceRequestRequest
        {
            ServiceTypeId = 1,
            BuildingId = 1,
            Priority = "Medium"
        }, "regular-user-id");

        Assert.NotNull(result.ServiceLevelAgreementDate);
        var expectedSla = DateTime.UtcNow.AddDays(3);
        Assert.True(result.ServiceLevelAgreementDate!.Value <= expectedSla.AddMinutes(1));
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _serviceRequestService.DeleteAsync(999, "admin-id", isAdmin: true));
    }

    [Fact]
    public async Task DeleteAsync_AsRegularUser_WhenNotOwner_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _serviceRequestService.DeleteAsync(1, "another-user-id", isAdmin: false));
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesRequest()
    {
        await _serviceRequestService.DeleteAsync(1, "regular-user-id", isAdmin: false);
        Assert.Equal(1, await _context.ServiceRequests.CountAsync());
    }

    [Fact]
    public async Task UpdateStatusAsync_AsRegularUser_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _serviceRequestService.UpdateStatusAsync(1, new UpdateServiceRequestStatusRequest
            {
                Status = "InProgress"
            }, "regular-user-id", isAdmin: false));
    }

    [Fact]
    public async Task UpdateStatusAsync_WithInvalidStatus_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _serviceRequestService.UpdateStatusAsync(1, new UpdateServiceRequestStatusRequest
            {
                Status = "InvalidStatus"
            }, "admin-id", isAdmin: true));
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidStatus_UpdatesRequestAndCreatesUpdate()
    {
        var result = await _serviceRequestService.UpdateStatusAsync(1, new UpdateServiceRequestStatusRequest
        {
            Status = "InProgress"
        }, "admin-id", isAdmin: true);

        Assert.Equal("InProgress", result.Status);
        Assert.True(await _context.Updates.AnyAsync(u =>
            u.ServiceRequestId == 1 && u.Type == UpdateType.Event));
    }

    [Fact]
    public async Task UpdateStatusAsync_WithComment_CreatesCommentUpdate()
    {
        await _serviceRequestService.UpdateStatusAsync(1, new UpdateServiceRequestStatusRequest
        {
            Status = "InProgress",
            Comment = "Working on it"
        }, "admin-id", isAdmin: true);

        Assert.True(await _context.Updates.AnyAsync(u =>
            u.ServiceRequestId == 1 && u.Type == UpdateType.Message));
    }

    [Fact]
    public async Task GetUserHomeDataAsync_ReturnsRecentRequestsAndServiceTypes()
    {
        var result = await _serviceRequestService.GetUserHomeDataAsync("regular-user-id");

        Assert.Single(result.RecentRequests);
        Assert.Single(result.RecentServiceTypes);
    }
}