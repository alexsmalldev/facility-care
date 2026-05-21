namespace FacilityCare.Domain.Entities;

public class BuildingUser
{
    public int BuildingId { get; set; }
    public Building Building { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
}