using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class TripService : ITripService
{
    private readonly ApbdContext _context;

    public TripService(ApbdContext context)
    {
        _context = context;
    }
    public async Task<GetTripsResponseDto> GetTrips(int page = 1, int pageSize = 10)
    {
        var totalTrips = await _context.Trips.CountAsync();
        var trips = await _context.Trips
            .OrderBy(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    
        return new GetTripsResponseDto
        {
            PageNum = page,
            PageSize = pageSize,
            AllPages = (int)Math.Ceiling((double)totalTrips / pageSize),
            Trips = trips.Select(t => new TripDto
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom.ToString("yyyy-MM-dd"),
                DateTo = t.DateTo.ToString("yyyy-MM-dd"),
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries
                    .Select(tc => new CountryDto { Name = _context.Countries.First(c => c.IdCountry == tc.IdCountry).Name })
                    .ToList(),
                Clients = _context.ClientTrips
                    .Join(_context.Clients, ct => ct.IdClient, c => c.IdClient, (ct, c) => new { ClientTrip = ct, Client = c })
                    .Where(joined => joined.ClientTrip.IdTrip == t.IdTrip)
                    .Select(joined => new ClientDto
                    {
                        FirstName = joined.Client.FirstName,
                        LastName = joined.Client.LastName
                    }).ToList()
            }).ToList()
          
        };
    }
    
    public async Task DeleteClient(int idClient)
    {
        var client = await _context.Clients.FindAsync(idClient);
        if (client == null)
            throw new KeyNotFoundException("Client not found");

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }
}