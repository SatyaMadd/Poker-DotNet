using pokerapi.Models;
namespace pokerapi.Interfaces{
    public interface IGameService
    {
        Task<IEnumerable<PlayerGameDTO>> GetPlayersAsync(string username);
        Task<GameDTO> GetGameAsync(string username);
        Task<IEnumerable<PlayerCard>> GetCardsAsync(string username);
        Task<IEnumerable<GameAction>> CheckAsync(string username);
        Task<IEnumerable<GameAction>> BetAsync(string username, int betAmount);
        Task<IEnumerable<GameAction>> FoldAsync(string username);
    }
}