using pokerapi.Models;
namespace pokerapi.Interfaces{
    public interface IGameService
    {
        Task<Player> GetPlayerAsync(string username);
        Task<IEnumerable<PlayerGameDTO>> GetAllPlayersAsync(string username);
        Task<GameDTO> GetGameAsync(string username);
        Task<IEnumerable<PlayerCard>> GetPlayerCardsAsync(string username);
        Task<IEnumerable<PlayerCardDTO>> GetAllPlayerCardsAsync(int gameId);
        Task<IEnumerable<GameAction>> CheckAsync(string username);
        Task<IEnumerable<GameAction>> BetAsync(string username, int betAmount);
        Task<IEnumerable<GameAction>> FoldAsync(string username);
        Task<IEnumerable<GameAction>> ShowCardsAsync(string username);
        Task<IEnumerable<GameAction>> MuckCardsAsync(string username);
        Task<string> UpdateGameTimeAsync(int gameId);
    }
}