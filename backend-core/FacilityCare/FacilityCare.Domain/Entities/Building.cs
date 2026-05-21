namespace FacilityCare.Domain.Entities;

public class Building
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string Postcode { get; set; } = string.Empty;
    public string Country { get; set; } = "United Kingdom";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public ICollection<BuildingUser> BuildingUsers { get; set; } = new List<BuildingUser>();
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}