namespace FacilityCare.Application.DTOs.Buildings;

public class CreateBuildingRequest
{
    public string? Name { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string Postcode { get; set; } = string.Empty;
    public string Country { get; set; } = "United Kingdom";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}