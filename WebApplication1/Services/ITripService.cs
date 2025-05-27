using WebApplication1.DTOs;

namespace WebApplication1.Services;

public interface ITripService
{
    Task<GetTripsResponseDto> GetTrips(int page = 1, int pageSize = 10);


}