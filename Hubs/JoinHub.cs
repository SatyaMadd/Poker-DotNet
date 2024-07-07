using Microsoft.AspNetCore.SignalR;
using pokerapi.Interfaces;
using pokerapi.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace pokerapi.Hubs
{
    [Authorize]
    public class JoinHub : Hub
    {
        private readonly ILobbyService _lobbyService;
        private readonly IGameService _gameService;
        private readonly IJoinService _joinService;
        private readonly IWaitingRoomService _waitingRoomService;

        public JoinHub(ILobbyService lobbyService, IGameService gameService, IWaitingRoomService waitingRoomService, IJoinService joinService)
        {
            _lobbyService = lobbyService;
            _gameService = gameService;
            _waitingRoomService = waitingRoomService;
            _joinService = joinService;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return;
            }
            var waitingPlayer = await _waitingRoomService.GetWaitingRoomPlayer(username);
            if(waitingPlayer==null){
                var player = await _gameService.GetPlayerAsync(username);
                if(player==null){
                    await Clients.Caller.SendAsync("GameListRefresh");
                    return;
                }else if(!(player.IsAdmin&&player.Ready)){
                    await Groups.AddToGroupAsync(Context.ConnectionId, player.GlobalVId.ToString());
                    await Clients.Caller.SendAsync("LobbyRefresh");
                    return;
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, player.GlobalVId.ToString());
                await Clients.Caller.SendAsync("WaitingRoomRefresh");
            }else{
                await Groups.AddToGroupAsync(Context.ConnectionId, waitingPlayer.GlobalVId.ToString());
                await Clients.Caller.SendAsync("WaitingRoomRefresh");
            }
            await base.OnConnectedAsync();
        }

        public async Task<IEnumerable<WaitingRoomPlayer>> GetWaitingRoomPlayers()
        {
            var username = Context.User?.Identity?.Name;
            if (username == null){
                return null;
            }
            var waitingPlayer = await _waitingRoomService.GetWaitingRoomPlayer(username);
            if(waitingPlayer==null){
                var player = await _gameService.GetPlayerAsync(username);
                if(player==null){
                    return null;
                }else if(!player.IsAdmin){
                    return null;
                }
                return await _waitingRoomService.GetAllWaitingRoomPlayers(player.GlobalVId);
            }else{
                return await _waitingRoomService.GetAllWaitingRoomPlayers(waitingPlayer.GlobalVId);
            }
        }

        public async Task<IEnumerable<JoinGameDTO>> GetAvailableGames(){
            return await _joinService.GetAvailableGamesAsync();
        }
        
        public async Task<bool> CreateGame(string gameName)
        {
            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                return false;
            }
            var player = await _gameService.GetPlayerAsync(username);
            if (player!=null){
                return true;
            }
            if (string.IsNullOrWhiteSpace(gameName))
            {
                return false;
            }
            var game = await _joinService.CreateGameAsync(gameName);
            if(game==null){
                return false;
            }
            await _joinService.JoinGameAsync(game.Id, username);
            await Clients.All.SendAsync("GameListRefresh");
            return true;
        }

        public async Task<string> JoinGame(int gameId)
        {
            var username = Context.User?.Identity?.Name; 
            if (username == null)
            {
                return null;
            }
            var location = await _joinService.JoinGameAsync(gameId, username);            
            if(location == null){
                await Clients.Caller.SendAsync("GameListRefresh");
                return null;
            }else{
                await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
                if(location=="Lobby"){
                    await Clients.Group(gameId.ToString()).SendAsync("LobbyRefresh");
                }else{
                   await Clients.Group(gameId.ToString()).SendAsync("WaitingRoomRefresh");
                }
            }
            return location;
        }
        public async Task StartGame()
        {
            var username = Context.User?.Identity?.Name; 
            var player = await _gameService.GetPlayerAsync(username);
            if (player==null){
                return;
            }
            var players = await _lobbyService.GetPlayersAsync(username);
            if (players.Count() == 1)
            {
                return;
            }
            await _lobbyService.AddDeckCardsAsync(username);
            await _lobbyService.DealCardsAsync(username, false);
            await _lobbyService.ReadyPlayersAsync(username);
            await Clients.Group(player.GlobalVId.ToString()).SendAsync("LobbyRefresh");
        }
        public async Task Leave()
        {
            var username = Context.User?.Identity?.Name;
            if (username == null)
            {
                return;
            }
            var player = await _gameService.GetPlayerAsync(username);
            int gameId;
            if (player == null)
            {
                var waitingPlayer = await _waitingRoomService.GetWaitingRoomPlayer(username);
                if (waitingPlayer == null){
                    return;
                }
                gameId = waitingPlayer.GlobalVId;
            }else{
                gameId = player.GlobalVId;
            }
            string location = await _lobbyService.LeaveGameAsync(username);
            if(location == "Lobby"){
                await Clients.Group(gameId.ToString()).SendAsync("LobbyRefresh");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId.ToString());
            }else if(location == "WaitingRoom"){
                await Clients.Group(gameId.ToString()).SendAsync("WaitingRoomRefresh");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId.ToString());
            }else if(location == "All"){
                await Clients.All.SendAsync("GameListRefresh");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId.ToString());
            }
        }
        
    }
}
