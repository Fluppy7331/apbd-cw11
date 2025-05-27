using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using TutorialWebApp.Exceptions;
using WebApplication1.Data;
using WebApplication1.DTOs;
using WebApplication1.Exceptions;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class TripService : ITripService
{
    private readonly ApbdContext _context;

    public TripService(ApbdContext context)
    {
        _context = context;
    }
    public async Task<GetTripsResponseDTO> GetTrips(int page = 1, int pageSize = 10)
    {
        var totalTrips = await _context.Trips.CountAsync();
        var trips = await _context.Trips
            .OrderBy(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    
        return new GetTripsResponseDTO
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
    //Bedzie sobie zwracać id clienta kótry stworzył żeby controller mogł zwrócić to w odpowiedzi
    public async Task<int> RegisterClientToTrip(int tripId, ClientRegistrationDTO clientDto)
    {
            var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == clientDto.Pesel);
            if (existingClient != null)
            {
                throw new NotFoundException("Klient o podanym numerze PESEL już istnieje.");
            }
            var isClientRegistered = await _context.ClientTrips.Join(_context.Clients, ct => ct.IdClient, c => c.IdClient, (ct, c) => new { ClientTrip = ct, Client = c })
                .AnyAsync(joined => joined.ClientTrip.IdTrip == tripId && joined.Client.Pesel == clientDto.Pesel);
            if (isClientRegistered)
            {
                throw new ConflictException("Klient o podanym numerze PESEL jest już zapisany na tę wycieczkę.");
            }
            
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.IdTrip == tripId);
            if (trip == null)
            {
                throw new ConflictException("Wycieczka o podanym ID nie istnieje.");
            }
            if (trip.DateFrom <= DateTime.Now)
            {
                throw new ConflictException("Nie można zapisać się na wycieczkę, która już się odbyła.");
            }
            
            var newClient = new Client
            {
                FirstName = clientDto.FirstName,
                LastName = clientDto.LastName,
                Pesel = clientDto.Pesel,
                Email = clientDto.Email,
                Telephone = clientDto.Telephone
            };
            _context.Clients.Add(newClient);
            await _context.SaveChangesAsync();
            var createdClientId = newClient.IdClient;
            
            var clientTrip = new ClientTrip
            {
                IdClient = createdClientId,
                IdTrip = tripId,
                RegisteredAt = DateTime.Now,
                PaymentDate = clientDto.PaymentDate
            };
            _context.ClientTrips.Add(clientTrip);
            await _context.SaveChangesAsync();
        
            return createdClientId;
        
    }
}