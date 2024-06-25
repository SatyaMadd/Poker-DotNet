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

        public async Task<IEnumerable<GlobalV>> GetAvailableGamesAsync()
        {
            return await _joinRepository.GetAvailableGamesAsync();
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
        public async Task JoinGameAsync(int gameId, string username)
        {
            var game = await _joinRepository.GetGameByIdAsync(gameId);
            if (game == null)
            {
                return;
            }

            var player = await _lobbyRepository.GetPlayer(username);
            if (player != null)
            {
                return;
            }

            var isFirstPlayer = game.Players.Count == 0;
            var newPlayer = new Player
            {
                Username = username,
                GlobalVId = gameId,
                IsAdmin = isFirstPlayer,
            };

            await _joinRepository.AddPlayerToGameAsync(newPlayer);
            return;
        }


    }
}
