using pokerapi.Interfaces;
using pokerapi.Models;

namespace pokerapi.Services
{
    public class JoinService : IJoinService
    {
        private readonly IJoinRepository _joinRepository;
        private readonly ILobbyRepository _lobbyRepository;

        public JoinService(IJoinRepository joinRepository, ILobbyRepository lobbyRepository)
        {
            _joinRepository = joinRepository;
            _lobbyRepository = lobbyRepository;
        }

        public async Task<IEnumerable<JoinGameDTO>> GetAvailableGamesAsync()
        {
            var games = await _joinRepository.GetAvailableGamesAsync();
            
            return games.Select(g => new JoinGameDTO
            {
                Id = g.Id,
                Name = g.Name,
                HasStarted = g.Players.Any(p => p.Ready),
                numPlayers = g.Players.Count
            }).AsEnumerable();

        }

        public async Task<GlobalV> CreateGameAsync(string name)
        {
            var games = await GetAvailableGamesAsync();
            if (games.Any(g => g.Name == name))
            {
                return null;
            }
            return await _joinRepository.CreateGameAsync(name);
        }
        public async Task<string> JoinGameAsync(int gameId, string username)
        {
            var game = await _joinRepository.GetGameByIdAsync(gameId);
            if (game == null)
            {
                return null;
            }

            var player = await _lobbyRepository.GetPlayer(username);
            if (player != null)
            {
                return "Lobby";
            }
            
            if (game.Players.Any(p => p.Ready))
            {
                var waitingRoomPlayer = await _lobbyRepository.GetWaitingRoomPlayer(username);
                if(waitingRoomPlayer!=null){
                    return "WaitingRoom";
                }
                await _joinRepository.AddPlayerToWaitingRoomAsync(gameId, username);
                return "WaitingRoom";
            }
            var isFirstPlayer = game.Players.Count == 0;
            var newPlayer = new Player
            {
                Username = username,
                GlobalVId = gameId,
                IsAdmin = isFirstPlayer,
            };

            await _joinRepository.AddPlayerToGameAsync(newPlayer);
            return "Lobby";
        }


    }
}
