using Microsoft.AspNetCore.Mvc;
using pokerapi.Models;
using pokerapi.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace pokerapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JoinController : ControllerBase
    {
        private readonly IJoinService _joinService;

        public JoinController(IJoinService joinService)
        {
            _joinService = joinService;
        }

        [HttpGet("GetAvailableGames")]
        public async Task<IActionResult> GetAvailableGames()
        {
            var games = await _joinService.GetAvailableGamesAsync();
            return Ok(games);
        }

        [HttpPost("CreateGame")]
        public async Task<IActionResult> CreateGame([FromBody] string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                return BadRequest("Game name is required.");
            }

            var game = await _joinService.CreateGameAsync(gameName);
            return Ok(game);
        }
        [Authorize]
        [HttpPost("JoinGame/{gameId}")]
        public async Task<IActionResult> JoinGame(int gameId)
        {
            
            var username = User.Identity?.Name; 
            if (username == null)
            {
                return Unauthorized();
            }
            await _joinService.JoinGameAsync(gameId, username);
            
            return Ok();
        }
    }
}
