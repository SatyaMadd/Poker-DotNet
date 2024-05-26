using pokerapi.Models;
namespace pokerapi.Interfaces
{
    public interface IJoinService
    {
        Task<IEnumerable<GlobalV>> GetAvailableGamesAsync();
        Task<GlobalV> CreateGameAsync(string name);
        Task<bool> JoinGameAsync(int gameId, string username);
    }
}
