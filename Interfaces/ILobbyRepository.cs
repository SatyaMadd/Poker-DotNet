using System.Collections.Generic;
using System.Threading.Tasks;
using pokerapi.Models;

namespace pokerapi.Interfaces
{
    public interface ILobbyRepository
    {
        Task<Player> GetPlayer(string username);
        Task ReadyPlayers(int gameId);
        Task DeletePlayer(string username);
        Task<GlobalV?> GetGameByIdAsync(int gameId);
        Task DeleteGame(int gameId);

    }
}