using Microsoft.AspNetCore.SignalR;
using pokerapi.Interfaces;
using pokerapi.Models;
using pokerapi.Hubs;

public class HubInteractionService : IHubInteractionService
{
    private readonly IHubContext<GameHub> _hubContext;
    private readonly IGameService _gameService;
    private readonly IBotService _botService;
    private readonly IServiceScopeFactory _serviceScopeFactory;


    public HubInteractionService(IHubContext<GameHub> hubContext, IGameService gameService, IBotService botService, IServiceScopeFactory serviceScopeFactory)
    {
        _hubContext = hubContext;
        _gameService = gameService;
        _botService = botService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ExecuteAction(Func<string, int, Task<IEnumerable<GameAction>>> actionFunc, string username, int amount = 0)
    {
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
                await _hubContext.Clients.Group(game.Id.ToString()).SendAsync("Send", action);
                var lastAction = actions.Last();
                if(lastAction == action)
                {
                    string curUser = await _gameService.UpdateGameTimeAsync(game.Id);
                    await _hubContext.Clients.Group(game.Id.ToString()).SendAsync("Refresh");
                    await Timer(curUser);
                }
            }
        }
    }

    public async Task HandleAutoCheck(string username)
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
                await _hubContext.Clients.Group(game.Id.ToString()).SendAsync("Send", action);
                var lastAction = actions.Last();
                if(lastAction == action)
                {
                    string curUser = await _gameService.UpdateGameTimeAsync(game.Id);
                    await _hubContext.Clients.Group(game.Id.ToString()).SendAsync("Refresh");
                    await Timer(curUser);
                }
            }
        }
    }

    public async Task HandleBotAction(string botName)
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
                await _hubContext.Clients.Group(game.Id.ToString()).SendAsync("Send", action);
                var lastAction = actions.Last();
                if(lastAction == action)
                {
                    string curUser = await _gameService.UpdateGameTimeAsync(game.Id);
                    await _hubContext.Clients.Group(game.Id.ToString()).SendAsync("Refresh");
                    await Timer(curUser);
                }
            }
        }
    }

    private async Task Timer(string username){
        var game = await _gameService.GetGameAsync(username);
        if (game == null){
            return;
        }
        await Task.Delay(30000);
        using(var scope = _serviceScopeFactory.CreateScope()){
            var _scopedGameService = scope.ServiceProvider.GetRequiredService<IGameService>();
            var _scopedLobbyService = scope.ServiceProvider.GetRequiredService<ILobbyService>();
            var _scopedHubContext = scope.ServiceProvider.GetRequiredService<IHubContext<GameHub>>();
            var _scopedHubInteractionService = scope.ServiceProvider.GetRequiredService<IHubInteractionService>();
            var updatedGame = await _scopedGameService.GetGameAsync(username);
            if (game == null){
                return;
            }
            TimeSpan timeElapsed = DateTime.Now - updatedGame.LastMoveTime;
            if (timeElapsed.TotalSeconds < 30)
            {
                return;
            }
            string botName = await _scopedLobbyService.LeaveGameAsync(username);
            GameAction botify = new GameAction{
                ActionName = "PlayerLeft",
                PlayerName = username
            };
            await _scopedHubContext.Clients.Group(updatedGame.Id.ToString()).SendAsync("Send", botify);
            if(botName != null){
                await _scopedHubInteractionService.HandleBotAction(botName);
            }else{
                await _scopedHubContext.Clients.Group(updatedGame.Id.ToString()).SendAsync("Refresh");
            }
            
        }
        
    }
}
