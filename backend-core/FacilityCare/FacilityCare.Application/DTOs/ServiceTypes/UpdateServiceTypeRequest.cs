namespace FacilityCare.Application.DTOs.ServiceTypes;

public class UpdateServiceTypeRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ServiceIcon { get; set; }
    public bool? IsActive { get; set; }
}