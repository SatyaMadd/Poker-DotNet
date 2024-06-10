using Microsoft.AspNetCore.Mvc;
using pokerapi.Models;
using pokerapi.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace pokerapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class LobbyController : ControllerBase
    {
        private readonly ILobbyService _lobbyService;

        public LobbyController(ILobbyService lobbyService)
        {
            _lobbyService = lobbyService;
        }

        [HttpPost("StartGame")]
        public async Task<IActionResult> StartGame()
        {
            var username = User.Identity?.Name;
            await _lobbyService.AddDeckCardsAsync(username);
            await _lobbyService.DealCardsAsync(username, false);
            await _lobbyService.ReadyPlayersAsync(username);
            return Ok();
        }

        [HttpPost("KickPlayer")]
        public async Task<IActionResult> KickPlayer([FromBody] string kickedUsername)
        {
            var username = User.Identity?.Name; 
            await _lobbyService.KickPlayerAsync(username, kickedUsername);
            return Ok();
        }

        [HttpPost("LeaveGame")]
        public async Task<IActionResult> LeaveGame()
        {
            var username = User.Identity?.Name; 
            await _lobbyService.LeaveGameAsync(username);
            return Ok();
        }
        
        [HttpGet("GetPlayers")]
        public async Task<IActionResult> GetPlayers()
        {
            var username = User.Identity?.Name; 
            var players = await _lobbyService.GetPlayersAsync(username);
            if(players == null){
                return NotFound();
            }
            return Ok(players);
        }
    }
}