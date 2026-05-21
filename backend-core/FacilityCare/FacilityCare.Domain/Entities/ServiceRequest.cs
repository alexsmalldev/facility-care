using FacilityCare.Domain.Enums;

namespace FacilityCare.Domain.Entities;

public class ServiceRequest
{
    public int Id { get; set; }
    public string? CustomerNotes { get; set; }
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Open;
    public ServiceRequestPriority Priority { get; set; } = ServiceRequestPriority.Low;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ServiceLevelAgreementDate { get; set; }

    public string CreatedById { get; set; } = string.Empty;

    public int ServiceTypeId { get; set; }
    public ServiceType ServiceType { get; set; } = null!;

    public int BuildingId { get; set; }
    public Building Building { get; set; } = null!;

    public ICollection<Update> Updates { get; set; } = new List<Update>();
}