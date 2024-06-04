using pokerapi.Interfaces;
using pokerapi.Models;


namespace pokerapi.Services{
    public class LobbyService : ILobbyService
    {
        private readonly ILobbyRepository _lobbyRepository;

        public LobbyService(ILobbyRepository lobbyRepository)
        {
            _lobbyRepository = lobbyRepository;
        }

        public async Task StartGameAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if (player.IsAdmin)
            {
                await _lobbyRepository.ReadyPlayers(player.GlobalVId);
            }
        }

        public async Task KickPlayerAsync(string username, string kickedUsername)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if (player.IsAdmin)
            {
                await _lobbyRepository.DeletePlayer(kickedUsername);
            }
        }
        public async Task LeaveGameAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            var game = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (game != null)
            {
                if (game.Players.Count == 1){
                    await _lobbyRepository.DeletePlayer(username);
                    await _lobbyRepository.DeleteGame(game.Id);
                }else{
                    await _lobbyRepository.DeletePlayer(username);
                }
            }          
        }
        public async Task<IEnumerable<PlayerAdmin>> GetPlayersAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if (player==null){
                return null;
            }
            var game = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (game == null)
            {
                return null;
            }
            var playerAdmins = game.Players.Select(player => new PlayerAdmin 
            { 
                Username = player.Username, 
                IsAdmin = player.IsAdmin 
            }).ToList();

            return playerAdmins;
        }
    }
}