using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using pokerapi.Models;
using pokerapi.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace pokerapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }
        [HttpGet("GetPlayers")]
        public async Task<IActionResult> GetPlayers()
        {
            var username = User.Identity?.Name; 
            var players = await _gameService.GetPlayersAsync(username);
            if(players == null){
                return NotFound();
            }
            return Ok(players);
        }
        [HttpGet("GetGame")]
        public async Task<IActionResult> GetGame()
        {
            var username = User.Identity?.Name; 
            var game  = await _gameService.GetGameAsync(username);
            if(game == null){
                return NotFound();
            }
            return Ok(game);
        }
        [HttpGet("GetPlayerCards")]
        public async Task<IActionResult> GetPlayerCards()
        {
            var username = User.Identity?.Name; 
            var playerCards = await _gameService.GetCardsAsync(username);
            if(playerCards == null){
                return NotFound();
            }
            return Ok(playerCards);
        }
        
        [HttpPost("Check")]
        public async Task<IActionResult> Check()
        {
            var username = User.Identity?.Name; 
            var actions = await _gameService.CheckAsync(username);
            return Ok(actions);
        }

        [HttpPost("Bet")]
        public async Task<IActionResult> Bet([FromBody] int amount)
        {
            var username = User.Identity?.Name; 
            var actions = await _gameService.BetAsync(username, amount);
            return Ok(actions);
        }

        [HttpPost("Fold")]
        public async Task<IActionResult> Fold()
        {
            var username = User.Identity?.Name; 
            var actions = await _gameService.FoldAsync(username);
            return Ok(actions);
        }
    }
}