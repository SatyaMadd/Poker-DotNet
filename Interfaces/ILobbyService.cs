using pokerapi.Models;
namespace pokerapi.Interfaces{
    public interface ILobbyService
    {
        Task StartGameAsync(string username);
        Task KickPlayerAsync(string username, string kickedUsername);
        Task LeaveGameAsync(string username);
        Task<IEnumerable<PlayerAdmin>> GetPlayersAsync(string username);
    }
}