namespace FacilityCare.Application.DTOs.ServiceTypes;

public class CreateServiceTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ServiceIcon { get; set; }
}