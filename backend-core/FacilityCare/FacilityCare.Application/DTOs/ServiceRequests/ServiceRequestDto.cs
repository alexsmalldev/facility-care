using FacilityCare.Application.DTOs.Buildings;
using FacilityCare.Application.DTOs.ServiceTypes;

namespace FacilityCare.Application.DTOs.ServiceRequests;

public class ServiceRequestDto
{
    public int Id { get; set; }
    public string? CustomerNotes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public DateTime? ServiceLevelAgreementDate { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public string CreatedByFirstName { get; set; } = string.Empty;
    public string CreatedByLastName { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentIntentId { get; set; }
    public ServiceTypeDto ServiceType { get; set; } = null!;
    public BuildingDto Building { get; set; } = null!;
}