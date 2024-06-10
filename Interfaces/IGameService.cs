using pokerapi.Models;
namespace pokerapi.Interfaces{
    public interface IGameService
    {
        Task<IEnumerable<PlayerGameDTO>> GetPlayersAsync(string username);
        Task<GameDTO> GetGameAsync(string username);
        Task<IEnumerable<PlayerCard>> GetCardsAsync(string username);
        Task CheckAsync(string username);
        Task BetAsync(string username, int betAmount);
        Task FoldAsync(string username);
    }
}