using System.Collections.Generic;
using System.Threading.Tasks;
using pokerapi.Models;

namespace pokerapi.Interfaces
{
    public interface IJoinRepository
    {
        Task<IEnumerable<GlobalV>> GetAvailableGamesAsync();
        Task<GlobalV> CreateGameAsync(string name);
        Task<GlobalV?> GetGameByIdAsync(int gameId);
        Task AddPlayerToGameAsync(Player player);
    }
}
