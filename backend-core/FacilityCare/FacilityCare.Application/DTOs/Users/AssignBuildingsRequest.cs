namespace FacilityCare.Application.DTOs.Users;

public class AssignBuildingsRequest
{
    public List<int> BuildingIds { get; set; } = new List<int>();
}