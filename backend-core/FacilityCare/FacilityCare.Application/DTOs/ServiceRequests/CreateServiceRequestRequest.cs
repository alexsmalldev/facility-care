namespace FacilityCare.Application.DTOs.ServiceRequests;

public class CreateServiceRequestRequest
{
    public string? CustomerNotes { get; set; }
    public string Priority { get; set; } = string.Empty;
    public int ServiceTypeId { get; set; }
    public int BuildingId { get; set; }
}