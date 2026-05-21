namespace FacilityCare.Application.DTOs.ServiceRequests;

public class UpdateServiceRequestStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
}