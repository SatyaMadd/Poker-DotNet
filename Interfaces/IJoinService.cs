using pokerapi.Models;
namespace pokerapi.Interfaces
{
    public interface IJoinService
    {
        Task<IEnumerable<JoinGameDTO>> GetAvailableGamesAsync();
        Task<GlobalV> CreateGameAsync(string name);
        Task<string> JoinGameAsync(int gameId, string username);
    }
}
