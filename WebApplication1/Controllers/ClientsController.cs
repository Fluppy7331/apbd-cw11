using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorialWebApp.Exceptions;
using WebApplication1.Services;
using WebApplication1.Exceptions;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }
        
        [HttpDelete("{clientId}")]
        public async Task<IActionResult> DeleteClient(int clientId)
        {
            if (clientId <= 0)
            {
                return BadRequest("Nieprawidłowy identyfikator klienta.");
            }

            try
            {
                await _clientService.DeleteClient(clientId);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            return Ok($"Klient o ID {clientId} został pomyślnie usunięty.");
        }
    }
}