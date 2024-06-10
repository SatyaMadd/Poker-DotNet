using pokerapi.Models;
namespace pokerapi.Interfaces{
    public interface ILobbyService
    {
        Task ReadyPlayersAsync(string username);
        Task AddDeckCardsAsync(string username);
        Task DealCardsAsync(string username, bool inGame);
        Task KickPlayerAsync(string username, string kickedUsername);
        Task LeaveGameAsync(string username);
        Task<IEnumerable<PlayerLobbyDTO>> GetPlayersAsync(string username);
    }
}