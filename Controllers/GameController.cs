using System.Linq;
using Microsoft.AspNetCore.Mvc;
using pokerapi.Interfaces;

namespace pokerapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }
    }
}