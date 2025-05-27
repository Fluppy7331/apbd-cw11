using Microsoft.AspNetCore.Mvc;
using TutorialWebApp.Exceptions;
using WebApplication1.DTOs;
using WebApplication1.Exceptions;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _tripService.GetTrips(page, pageSize);
        return Ok(result);
    }

    [HttpPost("{tripId}/clients")]
    public async Task<IActionResult> RegisterClientToTrip(int tripId, [FromBody] ClientRegistrationDTO clientDto)
    {
        if (tripId <= 0)
        {
            return BadRequest("Nieprawidłowy identyfikator wycieczki.");
        }

        int newClientId;
        try
        {
            newClientId = await _tripService.RegisterClientToTrip(tripId, clientDto);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }

        return Created(string.Empty, new { tripId, clientId = newClientId });
    }
}