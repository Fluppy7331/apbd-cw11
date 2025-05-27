using Microsoft.EntityFrameworkCore;
using TutorialWebApp.Exceptions;
using WebApplication1.Data;

namespace WebApplication1.Services;

public class ClientService : IClientService
{
    private readonly ApbdContext _context;

    public ClientService(ApbdContext context)
    {
        _context = context;
    }

    public async Task DeleteClient(int idClient)
    {
        var client = await _context.Clients.FindAsync(idClient);
        if (client == null)
        {
            throw new ConflictException($"Klient o ID {idClient} nie istnieje.");
        }
        var trips = await _context.ClientTrips.FirstOrDefaultAsync(ct => ct.IdClient == idClient);
        if (trips != null)
        {
            throw new ConflictException($"Klient o ID {idClient} ma powiązane wycieczki i nie może zostać usunięty.");
        }
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
      
    }
}