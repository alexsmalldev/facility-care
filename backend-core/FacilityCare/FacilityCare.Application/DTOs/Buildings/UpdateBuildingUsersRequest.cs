namespace FacilityCare.Application.DTOs.Buildings;

public class UpdateBuildingUsersRequest
{
    public List<string> UserIds { get; set; } = new List<string>();
}