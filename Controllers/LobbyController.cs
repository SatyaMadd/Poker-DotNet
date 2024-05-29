using System.Linq;
using Microsoft.AspNetCore.Mvc;
using pokerapi.Interfaces;

namespace pokerapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LobbyController : ControllerBase
    {
        private readonly ILobbyService _lobbyService;

        public LobbyController(ILobbyService lobbyService)
        {
            _lobbyService = lobbyService;
        }
    }
}