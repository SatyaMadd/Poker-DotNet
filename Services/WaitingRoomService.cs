using pokerapi.Interfaces;
using pokerapi.Models;


namespace pokerapi.Services{
    public class WaitingRoomService : IWaitingRoomService
    {
        private readonly ILobbyRepository _lobbyRepository;

        public WaitingRoomService(ILobbyRepository lobbyRepository)
        {
            _lobbyRepository = lobbyRepository;
        }

        public async Task<WaitingRoomPlayer> GetWaitingRoomPlayer(string username){
            return await _lobbyRepository.GetWaitingRoomPlayer(username);
        }

        public async Task<IEnumerable<WaitingRoomPlayer>> GetAllWaitingRoomPlayers(int gameId){
            return await _lobbyRepository.GetAllWaitingRoomPlayers(gameId);
        }
    }
}