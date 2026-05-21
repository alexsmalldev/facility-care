using FacilityCare.Application.Common.Interfaces;
using FacilityCare.Application.DTOs.ServiceTypes;
using FacilityCare.Domain.Entities;
using FacilityCare.Infrastructure.Persistence;
using FacilityCare.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FacilityCare.Application.Tests.Services;

public class ServiceTypeServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly ServiceTypeService _serviceTypeService;

    public ServiceTypeServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _fileServiceMock = new Mock<IFileService>();
        _serviceTypeService = new ServiceTypeService(_context, _fileServiceMock.Object);

        SeedData();
    }

    private void SeedData()
    {
        _context.ServiceTypes.AddRange(
            new ServiceType
            {
                Id = 1,
                Name = "Cleaning",
                Description = "Cleaning services",
                IsActive = true
            },
            new ServiceType
            {
                Id = 2,
                Name = "Maintenance",
                Description = "Maintenance services",
                IsActive = true
            },
            new ServiceType
            {
                Id = 3,
                Name = "Inactive Service",
                Description = "This service is inactive",
                IsActive = false
            }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllAsync_AsAdmin_ReturnsAllServiceTypes()
    {
        var result = await _serviceTypeService.GetAllAsync(isAdmin: true, search: null);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_AsRegularUser_ReturnsOnlyActiveServiceTypes()
    {
        var result = await _serviceTypeService.GetAllAsync(isAdmin: false, search: null);
        Assert.Equal(2, result.Count);
        Assert.All(result, st => Assert.True(st.IsActive));
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_ReturnsMatchingServiceTypes()
    {
        var result = await _serviceTypeService.GetAllAsync(isAdmin: true, search: "Cleaning");
        Assert.Single(result);
        Assert.Equal("Cleaning", result[0].Name);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_NoResults_ReturnsEmpty()
    {
        var result = await _serviceTypeService.GetAllAsync(isAdmin: true, search: "Nonexistent");
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _serviceTypeService.GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsServiceTypeDto()
    {
        var result = await _serviceTypeService.GetByIdAsync(1);
        Assert.Equal("Cleaning", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WithoutIcon_CreatesServiceType()
    {
        var result = await _serviceTypeService.CreateAsync(new CreateServiceTypeRequest
        {
            Name = "New Service",
            Description = "New service description"
        }, icon: null);

        Assert.Equal("New Service", result.Name);
        Assert.True(result.IsActive);
        Assert.Null(result.ServiceIcon);
        Assert.Equal(4, await _context.ServiceTypes.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_WithIcon_UploadsAndCreatesServiceType()
    {
        var iconMock = new Mock<IFormFile>();
        iconMock.Setup(f => f.FileName).Returns("icon.png");
        iconMock.Setup(f => f.ContentType).Returns("image/png");
        iconMock.Setup(f => f.Length).Returns(1024);

        _fileServiceMock.Setup(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("/uploads/service-icons/icon.png");

        var result = await _serviceTypeService.CreateAsync(new CreateServiceTypeRequest
        {
            Name = "Service With Icon",
            Description = "Has an icon"
        }, icon: iconMock.Object);

        Assert.Equal("/uploads/service-icons/icon.png", result.ServiceIcon);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _serviceTypeService.UpdateAsync(999, new UpdateServiceTypeRequest(), icon: null));
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_UpdatesServiceType()
    {
        var result = await _serviceTypeService.UpdateAsync(1, new UpdateServiceTypeRequest
        {
            Name = "Updated Cleaning",
            IsActive = false
        }, icon: null);

        Assert.Equal("Updated Cleaning", result.Name);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_WithNewIcon_DeletesOldAndUploadsNew()
    {
        _context.ServiceTypes.Find(1)!.ServiceIcon = "/uploads/service-icons/old-icon.png";
        await _context.SaveChangesAsync();

        var iconMock = new Mock<IFormFile>();
        iconMock.Setup(f => f.FileName).Returns("new-icon.png");
        iconMock.Setup(f => f.ContentType).Returns("image/png");
        iconMock.Setup(f => f.Length).Returns(1024);

        _fileServiceMock.Setup(f => f.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("/uploads/service-icons/new-icon.png");

        var result = await _serviceTypeService.UpdateAsync(1, new UpdateServiceTypeRequest(), icon: iconMock.Object);

        _fileServiceMock.Verify(f => f.DeleteFileAsync("/uploads/service-icons/old-icon.png"), Times.Once);
        Assert.Equal("/uploads/service-icons/new-icon.png", result.ServiceIcon);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ThrowsException()
    {
        await Assert.ThrowsAsync<Exception>(() =>
            _serviceTypeService.DeleteAsync(999));
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesServiceType()
    {
        await _serviceTypeService.DeleteAsync(1);
        Assert.Equal(2, await _context.ServiceTypes.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_WithIcon_DeletesFileAndServiceType()
    {
        _context.ServiceTypes.Find(1)!.ServiceIcon = "/uploads/service-icons/icon.png";
        await _context.SaveChangesAsync();

        await _serviceTypeService.DeleteAsync(1);

        _fileServiceMock.Verify(f => f.DeleteFileAsync("/uploads/service-icons/icon.png"), Times.Once);
        Assert.Equal(2, await _context.ServiceTypes.CountAsync());
    }
}