using FacilityCare.Domain.Enums;

namespace FacilityCare.Domain.Entities;

public class Update
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public UpdateType Type { get; set; } = UpdateType.Event;
    public bool IsRead { get; set; } = false;

    public string CreatedById { get; set; } = string.Empty;
    public string? AssociatedToId { get; set; }

    public int ServiceRequestId { get; set; }
    public ServiceRequest ServiceRequest { get; set; } = null!;
}