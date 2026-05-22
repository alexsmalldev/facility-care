namespace FacilityCare.Application.DTOs.ServiceTypes;

public class ServiceTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ServiceIcon { get; set; }
    public bool IsActive { get; set; }
    public bool IsPaid { get; set; }
    public decimal? Price { get; set; }
}