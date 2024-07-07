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
        private readonly IAuthService _AuthService;

        public JoinController(IJoinService joinService, IAuthService authService)
        {
            _joinService = joinService;
            _AuthService = authService;
        }

        [HttpGet("GetAvailableGames")]
        public async Task<IActionResult> GetAvailableGames()
        {
            var games = await _joinService.GetAvailableGamesAsync();
            return Ok(games);
        }

        [HttpPost("CreateGame")]
        [Authorize]
        public async Task<IActionResult> CreateGame([FromBody] string gameName)
        {
            var username = User.Identity?.Name; 
            if (username == null)
            {
                return Unauthorized();
            }
            var playerExists = await _AuthService.PlayerExists(username);
            if (playerExists){
                return BadRequest("Already in game.");
            }
            if (string.IsNullOrWhiteSpace(gameName))
            {
                return BadRequest("Game name is required.");
            }

            var game = await _joinService.CreateGameAsync(gameName);
            if(game==null){
                return BadRequest("Game name already exists.");
            }
            await _joinService.JoinGameAsync(game.Id, username);
            return Ok();
        }
        
        [HttpPost("JoinGame/{gameId}")]
        [Authorize]
        public async Task<IActionResult> JoinGame(int gameId)
        {
            
            var username = User.Identity?.Name; 
            if (username == null)
            {
                return Unauthorized();
            }
            var location = await _joinService.JoinGameAsync(gameId, username);
            if(location == null){
                return BadRequest("Game not found.");
            }
            
            return Ok(location);
        }
    }
}
