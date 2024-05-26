using pokerapi.Interfaces;
using pokerapi.Models;
using pokerapi.Repositories;

namespace pokerapi.Services
{
    public class JoinService : IJoinService
    {
        private readonly IJoinRepository _joinRepository;

        public JoinService(IJoinRepository joinRepository)
        {
            _joinRepository = joinRepository;
        }

        public async Task<IEnumerable<GlobalV>> GetAvailableGamesAsync()
        {
            return await _joinRepository.GetAvailableGamesAsync();
        }

        public async Task<GlobalV> CreateGameAsync(string name)
        {
            return await _joinRepository.CreateGameAsync(name);
        }
        public async Task<bool> JoinGameAsync(int gameId, string username)
        {
            // Check if the game exists
            var game = await _joinRepository.GetGameByIdAsync(gameId);
            if (game == null)
            {
                // The game does not exist
                return false;
            }

            // Check if the player already exists in the game
            var playerExists = await _joinRepository.CheckPlayerExistsInGameAsync(gameId, username);
            if (playerExists)
            {
                // The player already exists in this game
                return true;
            }

            // Create a new player and link to the game
            var player = new Player
            {
                Username = username,
                GameId = gameId
                // Initialize other properties as needed
            };

            await _joinRepository.AddPlayerToGameAsync(player);
            return true;
        }

    }
}
