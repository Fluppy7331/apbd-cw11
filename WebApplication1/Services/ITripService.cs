using WebApplication1.DTOs;

namespace WebApplication1.Services;

public interface ITripService
{
    Task<GetTripsResponseDTO> GetTrips(int page = 1, int pageSize = 10);
    Task<int> RegisterClientToTrip(int tripId, ClientRegistrationDTO clientDto);
}