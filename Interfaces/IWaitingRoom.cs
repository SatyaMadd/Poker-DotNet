using pokerapi.Models;
namespace pokerapi.Interfaces{
    public interface IWaitingRoomService
    {
        Task<WaitingRoomPlayer> GetWaitingRoomPlayer(string username);
        Task<IEnumerable<WaitingRoomPlayer>> GetAllWaitingRoomPlayers(int gameId);
        Task AdmitPlayer(string username);
    }
}