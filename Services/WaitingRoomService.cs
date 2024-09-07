using pokerapi.Interfaces;
using pokerapi.Models;


namespace pokerapi.Services{
    public class WaitingRoomService : IWaitingRoomService
    {
        private readonly IJoinRepository _joinRepository;
        private readonly ILobbyRepository _lobbyRepository;
        

        public WaitingRoomService(ILobbyRepository lobbyRepository, IJoinRepository joinRepository)
        {
            _joinRepository = joinRepository;
            _lobbyRepository = lobbyRepository;
        }

        public async Task<WaitingRoomPlayer> GetWaitingRoomPlayer(string username){
            return await _lobbyRepository.GetWaitingRoomPlayer(username);
        }

        public async Task<IEnumerable<WaitingRoomPlayer>> GetAllWaitingRoomPlayers(int gameId){
            return await _lobbyRepository.GetAllWaitingRoomPlayers(gameId);
        }

        public async Task AdmitPlayer(string username){
            var waitingPlayer = await _lobbyRepository.GetWaitingRoomPlayer(username);
            if(waitingPlayer==null){
                return;
            }
            await _lobbyRepository.RemoveWaitingRoomPlayer(waitingPlayer.Id);
            var game = await _joinRepository.GetGameByIdAsync(waitingPlayer.GlobalVId);
            if(game==null){
                return;
            }
            var latestTurnOrder = game.Players.OrderByDescending(p => p.TurnOrder).FirstOrDefault()?.TurnOrder ?? 0;
            var player = new Player
            {
                Username = waitingPlayer.Username,
                GlobalVId = waitingPlayer.GlobalVId,
                Chips = waitingPlayer.ChipsRequested,
                Ready = true,
                TurnOrder = latestTurnOrder + 1
            
            };
            await _joinRepository.AddPlayerToGameAsync(player);
            await _lobbyRepository.InitializePlayerBet(player.Id, player.GlobalVId);
        }
    }
}