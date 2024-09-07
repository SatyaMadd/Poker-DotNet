using Microsoft.AspNetCore.SignalR;
using pokerapi.Interfaces;
using pokerapi.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace pokerapi.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private readonly IHubInteractionService _hubInteractionService;
        private readonly IGameService _gameService;
        private readonly ILobbyService _lobbyService;

        public GameHub(IHubInteractionService hubInteractionService, IGameService gameService, ILobbyService lobbyService)
        {
            _hubInteractionService = hubInteractionService;
            _gameService = gameService;
            _lobbyService = lobbyService;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return;
            }
            var game = await _gameService.GetGameAsync(username);
            if (game == null){
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, game.Id.ToString());
            await Clients.Caller.SendAsync("Refresh");
            await base.OnConnectedAsync();
        }

        public async Task Check()
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return;
            }
            _hubInteractionService.ExecuteAction((username, _) => _gameService.CheckAsync(username), username);
        }

        public async Task Bet(int amount)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return;
            }
            _hubInteractionService.ExecuteAction(_gameService.BetAsync, username, amount);
        }

        public async Task Fold()
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return;
            }
            _hubInteractionService.ExecuteAction((username, _) => _gameService.FoldAsync(username), username);
        }

        public async Task ShowCards()
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return;
            }
            _hubInteractionService.ExecuteAction((username, _) => _gameService.ShowCardsAsync(username), username);
        }

        public async Task MuckCards()
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return;
            }
            _hubInteractionService.ExecuteAction((username, _) => _gameService.MuckCardsAsync(username), username);
        }

        public async Task Leave()
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return;
            }
            var game = await _gameService.GetGameAsync(username);
            if (game == null){
                return;
            }
            string botName = await _lobbyService.LeaveGameAsync(username);
            GameAction botify = new GameAction{
                ActionName = "PlayerLeft",
                PlayerName = username
            };
            await Clients.Group(game.Id.ToString()).SendAsync("Send", botify);
            if(botName != null){
                await _hubInteractionService.HandleBotAction(botName);
            }else{
                await Clients.Group(game.Id.ToString()).SendAsync("Refresh");
            }
        }
        public async Task<object> Refresh()
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return null;
            }
            var game = await _gameService.GetGameAsync(username);
            if (game == null){
                return null;
            }
            
            var players = await _gameService.GetAllPlayersAsync(username);
            var playerCards = await _gameService.GetPlayerCardsAsync(username);
            var allPlayerCards = await _gameService.GetAllPlayerCardsAsync(game.Id);
            var response = new 
            {
                game,
                players,
                playerCards,
                allPlayerCards
            };
            return await Task.FromResult(response);
        }
        public async Task Kick(string kickedUsername)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return;
            }
            var game = await _gameService.GetGameAsync(username);
            if (game == null){
                return;
            }
            string botName = await _lobbyService.LeaveGameAsync(kickedUsername);
            if (botName == null || !botName.StartsWith("BotLeave")) {
                return;
            }            
            GameAction botify = new GameAction{
                ActionName = "PlayerLeft",
                PlayerName = kickedUsername
            };
            await Clients.Group(game.Id.ToString()).SendAsync("Send", botify);
            if(botName != null){
                await _hubInteractionService.HandleBotAction(botName);
            }else{
                await Clients.Group(game.Id.ToString()).SendAsync("Refresh");
            }
        }
    }
}
