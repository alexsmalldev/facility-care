using FacilityCare.Application.DTOs.ServiceRequests;
using FacilityCare.Application.DTOs.Updates;

namespace FacilityCare.Application.DTOs.Dashboard;

public class RequestsOverTimeDto
{
    public DateTime Day { get; set; }
    public int Count { get; set; }
}

public class RequestsByBuildingDto
{
    public string BuildingName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RequestsByServiceTypeDto
{
    public string ServiceTypeName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class GeneralStatsDto
{
    public int OpenRequests { get; set; }
    public int InProgressRequests { get; set; }
    public int CompletedRequests { get; set; }
}

public class DashboardRequestsOverTimeResponse
{
    public IList<RequestsOverTimeDto> RequestsOverTime { get; set; } = new List<RequestsOverTimeDto>();
}

public class DashboardTodaysUpdatesResponse
{
    public IList<UpdateDto> UpdatesToday { get; set; } = new List<UpdateDto>();
}

public class DashboardActionRequiredResponse
{
    public IList<ServiceRequestDto> ActionsRequired { get; set; } = new List<ServiceRequestDto>();
}

public class DashboardRequestsByBuildingResponse
{
    public IList<RequestsByBuildingDto> RequestsByBuilding { get; set; } = new List<RequestsByBuildingDto>();
}

public class DashboardRequestsByServiceTypeResponse
{
    public IList<RequestsByServiceTypeDto> RequestsByServiceType { get; set; } = new List<RequestsByServiceTypeDto>();
}

public class DashboardGeneralStatsResponse
{
    public GeneralStatsDto Stats { get; set; } = new GeneralStatsDto();
}