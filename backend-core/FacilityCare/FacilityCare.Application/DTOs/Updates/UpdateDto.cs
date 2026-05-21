namespace FacilityCare.Application.DTOs.Updates;

public class UpdateDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public string CreatedByFirstName { get; set; } = string.Empty;
    public string CreatedByLastName { get; set; } = string.Empty;
    public string? AssociatedToId { get; set; }
    public int ServiceRequestId { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}