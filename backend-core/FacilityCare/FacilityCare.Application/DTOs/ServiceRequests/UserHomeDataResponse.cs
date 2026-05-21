using FacilityCare.Application.DTOs.ServiceTypes;

namespace FacilityCare.Application.DTOs.ServiceRequests;

public class UserHomeDataResponse
{
    public IList<ServiceRequestDto> RecentRequests { get; set; } = new List<ServiceRequestDto>();
    public IList<ServiceTypeDto> RecentServiceTypes { get; set; } = new List<ServiceTypeDto>();
}