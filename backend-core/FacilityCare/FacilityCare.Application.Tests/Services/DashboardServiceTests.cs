using FacilityCare.Domain.Entities;
using FacilityCare.Domain.Enums;
using FacilityCare.Infrastructure.Persistence;
using FacilityCare.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FacilityCare.Application.Tests.Services;

public class DashboardServiceTests
{
    private readonly AppDbContext _context;
    private readonly DashboardService _dashboardService;

    public DashboardServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _dashboardService = new DashboardService(_context);

        SeedData();
    }

    private void SeedData()
    {
        _context.ServiceTypes.Add(new ServiceType
        {
            Id = 1,
            Name = "Test Service",
            Description = "Test Description",
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

        var statuses = new[] { ServiceRequestStatus.Open, ServiceRequestStatus.InProgress, ServiceRequestStatus.Completed };
        var priorities = new[] { ServiceRequestPriority.Low, ServiceRequestPriority.Medium, ServiceRequestPriority.High };

        for (int i = 0; i < 30; i++)
        {
            _context.ServiceRequests.Add(new ServiceRequest
            {
                CustomerNotes = $"Test request {i}",
                Status = statuses[i % 3],
                Priority = priorities[i % 3],
                CreatedById = "admin-id",
                ServiceTypeId = 1,
                BuildingId = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-(30 - i)),
                ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(5)
            });
        }

        for (int i = 0; i < 5; i++)
        {
            _context.Updates.Add(new Update
            {
                Message = $"Test update {i}",
                CreatedById = "admin-id",
                ServiceRequestId = 1,
                Type = UpdateType.Message,
                CreatedDate = DateTime.UtcNow
            });
        }

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetRequestsOverTimeAsync_With7Days_ReturnsCorrectRange()
    {
        var result = await _dashboardService.GetRequestsOverTimeAsync("7days");
        Assert.NotNull(result.RequestsOverTime);
        Assert.All(result.RequestsOverTime, r =>
            Assert.True(r.Day >= DateTime.UtcNow.AddDays(-7).Date));
    }

    [Fact]
    public async Task GetRequestsOverTimeAsync_With30Days_ReturnsCorrectRange()
    {
        var result = await _dashboardService.GetRequestsOverTimeAsync("30days");
        Assert.NotNull(result.RequestsOverTime);
        Assert.All(result.RequestsOverTime, r =>
            Assert.True(r.Day >= DateTime.UtcNow.AddDays(-30).Date));
    }

    [Fact]
    public async Task GetRequestsOverTimeAsync_Default_Returns90DayRange()
    {
        var result = await _dashboardService.GetRequestsOverTimeAsync("3months");
        Assert.NotNull(result.RequestsOverTime);
        Assert.All(result.RequestsOverTime, r =>
            Assert.True(r.Day >= DateTime.UtcNow.AddDays(-90).Date));
    }

    [Fact]
    public async Task GetTodaysUpdatesAsync_ReturnsOnlyTodaysMessageUpdates()
    {
        var result = await _dashboardService.GetTodaysUpdatesAsync();
        Assert.Equal(5, result.UpdatesToday.Count);
        Assert.All(result.UpdatesToday, u =>
        {
            Assert.Equal(DateTime.UtcNow.Date, u.CreatedDate.Date);
            Assert.Equal("Message", u.Type);
        });
    }

    [Fact]
    public async Task GetTodaysUpdatesAsync_DoesNotReturnYesterdaysUpdates()
    {
        _context.Updates.Add(new Update
        {
            Message = "Yesterday's update",
            CreatedById = "admin-id",
            ServiceRequestId = 1,
            Type = UpdateType.Message,
            CreatedDate = DateTime.UtcNow.AddDays(-1)
        });
        await _context.SaveChangesAsync();

        var result = await _dashboardService.GetTodaysUpdatesAsync();
        Assert.Equal(5, result.UpdatesToday.Count);
    }

    [Fact]
    public async Task GetActionRequiredAsync_ReturnsUrgentAndOverdueRequests()
    {
        _context.ServiceRequests.AddRange(
            new ServiceRequest
            {
                CustomerNotes = "Urgent request",
                Status = ServiceRequestStatus.Open,
                Priority = ServiceRequestPriority.High,
                CreatedById = "admin-id",
                ServiceTypeId = 1,
                BuildingId = 1,
                ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(2)
            },
            new ServiceRequest
            {
                CustomerNotes = "Overdue request",
                Status = ServiceRequestStatus.InProgress,
                Priority = ServiceRequestPriority.High,
                CreatedById = "admin-id",
                ServiceTypeId = 1,
                BuildingId = 1,
                ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(-1)
            },
            new ServiceRequest
            {
                CustomerNotes = "Far future request",
                Status = ServiceRequestStatus.Open,
                Priority = ServiceRequestPriority.Low,
                CreatedById = "admin-id",
                ServiceTypeId = 1,
                BuildingId = 1,
                ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(10)
            }
        );
        await _context.SaveChangesAsync();

        var result = await _dashboardService.GetActionRequiredAsync();

        Assert.Contains(result.ActionsRequired, r => r.CustomerNotes == "Urgent request");
        Assert.Contains(result.ActionsRequired, r => r.CustomerNotes == "Overdue request");
        Assert.DoesNotContain(result.ActionsRequired, r => r.CustomerNotes == "Far future request");
    }

    [Fact]
    public async Task GetActionRequiredAsync_DoesNotReturnCompletedRequests()
    {
        _context.ServiceRequests.Add(new ServiceRequest
        {
            CustomerNotes = "Completed request",
            Status = ServiceRequestStatus.Completed,
            Priority = ServiceRequestPriority.High,
            CreatedById = "admin-id",
            ServiceTypeId = 1,
            BuildingId = 1,
            ServiceLevelAgreementDate = DateTime.UtcNow.AddDays(1)
        });
        await _context.SaveChangesAsync();

        var result = await _dashboardService.GetActionRequiredAsync();
        Assert.DoesNotContain(result.ActionsRequired, r => r.CustomerNotes == "Completed request");
    }

    [Fact]
    public async Task GetRequestsByBuildingAsync_ReturnsCorrectCounts()
    {
        var result = await _dashboardService.GetRequestsByBuildingAsync();

        Assert.Single(result.RequestsByBuilding);
        Assert.Equal("Test Building", result.RequestsByBuilding[0].BuildingName);
        Assert.Equal(30, result.RequestsByBuilding[0].Count);
    }

    [Fact]
    public async Task GetRequestsByServiceTypeAsync_ReturnsCorrectCounts()
    {
        var result = await _dashboardService.GetRequestsByServiceTypeAsync();

        Assert.Single(result.RequestsByServiceType);
        Assert.Equal("Test Service", result.RequestsByServiceType[0].ServiceTypeName);
        Assert.Equal(30, result.RequestsByServiceType[0].Count);
    }

    [Fact]
    public async Task GetGeneralStatsAsync_ReturnsCorrectCounts()
    {
        var result = await _dashboardService.GetGeneralStatsAsync();

        Assert.Equal(10, result.Stats.OpenRequests);
        Assert.Equal(10, result.Stats.InProgressRequests);
        Assert.Equal(10, result.Stats.CompletedRequests);
    }
}