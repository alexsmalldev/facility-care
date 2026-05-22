namespace FacilityCare.Domain.Entities;

public class ServiceType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ServiceIcon { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPaid { get; set; } = false;
    public decimal? Price { get; set; }

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}