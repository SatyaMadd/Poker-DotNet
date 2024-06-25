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
        private readonly ILobbyService _lobbyService;
        private readonly IGameService _gameService;
        private readonly IBotService _botService;

        public GameHub(ILobbyService lobbyService, IGameService gameService, IBotService botService)
        {
            _lobbyService = lobbyService;
            _gameService = gameService;
            _botService = botService;
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
            await base.OnConnectedAsync();
        }
        private async Task ExecuteAction(Func<string, int, Task<IEnumerable<GameAction>>> actionFunc, int amount = 0)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return;
            }
            var game = await _gameService.GetGameAsync(username);
            if (game == null){
                return;
            }
            var actions = await actionFunc(username, amount);
            foreach (var action in actions){
                if(action.ActionName == "AutoCheck"){
                    await HandleAutoCheck(action.PlayerName);
                }
                else if(action.ActionName == "BotTurn"){
                    await HandleBotAction(action.PlayerName);
                }else{
                    await Clients.Group(game.Id.ToString()).SendAsync("Send", action);
                    var lastAction = actions.Last();
                    if(lastAction == action)
                    {
                        await Clients.Group(game.Id.ToString()).SendAsync("Send", new GameAction{ActionName = "Refresh"});
                    }
                }
            }
        }

        public async Task Check()
        {
            await ExecuteAction((username, _) => _gameService.CheckAsync(username));
        }

        public async Task Bet(int amount)
        {
            await ExecuteAction(_gameService.BetAsync, amount);
        }

        public async Task Fold()
        {
            await ExecuteAction((username, _) => _gameService.FoldAsync(username));
        }

        private async Task HandleBotAction(string botName)
        {
            var game = await _gameService.GetGameAsync(botName);
            if (game == null){
                return;
            }
            var actions = await _botService.BotMove(botName);
            foreach (var action in actions){
                if(action.ActionName == "AutoCheck"){
                    await HandleAutoCheck(action.PlayerName);
                }
                else if(action.ActionName == "BotTurn"){
                    await HandleBotAction(action.PlayerName);
                }else{
                    await Clients.Group(game.Id.ToString()).SendAsync("Send", action);
                    var lastAction = actions.Last();
                    if(lastAction == action)
                    {
                        await Clients.Group(game.Id.ToString()).SendAsync("Send", new GameAction{ActionName = "Refresh"});
                    }
                }
            }
        }
        private async Task HandleAutoCheck(string username)
        {
            var game = await _gameService.GetGameAsync(username);
            if (game == null){
                return;
            }
            var actions = await _gameService.CheckAsync(username);
            foreach (var action in actions){
                if(action.ActionName == "AutoCheck"){
                    await HandleAutoCheck(action.PlayerName);
                }
                else if(action.ActionName == "BotTurn"){
                    await HandleBotAction(action.PlayerName);
                }else{
                    await Clients.Group(game.Id.ToString()).SendAsync("Send", action);
                    var lastAction = actions.Last();
                    if(lastAction == action)
                    {
                        await Clients.Group(game.Id.ToString()).SendAsync("Send", new GameAction{ActionName = "Refresh"});
                    }
                }
            }
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
                await HandleBotAction(botName);
            }
        }
    }
}
