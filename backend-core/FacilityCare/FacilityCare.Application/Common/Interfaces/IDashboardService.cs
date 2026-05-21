using FacilityCare.Application.DTOs.Dashboard;

namespace FacilityCare.Application.Common.Interfaces;

public interface IDashboardService
{
    Task<DashboardRequestsOverTimeResponse> GetRequestsOverTimeAsync(string timeframe);
    Task<DashboardTodaysUpdatesResponse> GetTodaysUpdatesAsync();
    Task<DashboardActionRequiredResponse> GetActionRequiredAsync();
    Task<DashboardRequestsByBuildingResponse> GetRequestsByBuildingAsync();
    Task<DashboardRequestsByServiceTypeResponse> GetRequestsByServiceTypeAsync();
    Task<DashboardGeneralStatsResponse> GetGeneralStatsAsync();
}