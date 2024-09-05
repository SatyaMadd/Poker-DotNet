using pokerapi.Interfaces;
using pokerapi.Models;


namespace pokerapi.Services{
    public class LobbyService : ILobbyService
    {
        private readonly ILobbyRepository _lobbyRepository;
        private readonly IGameRepository _gameRepository;

        public LobbyService(ILobbyRepository lobbyRepository, IGameRepository gameRepository)
        {
            _lobbyRepository = lobbyRepository;
            _gameRepository = gameRepository;
        }

        public async Task ReadyPlayersAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if (player.IsAdmin&&player.Ready==false)
            {
                await _lobbyRepository.InitializeTurnOrder(player.GlobalVId);
                await _lobbyRepository.ReadyPlayers(player.GlobalVId);
                await _lobbyRepository.InitializeBets(player.GlobalVId);
                var globalV = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
                if(globalV==null){
                    return;
                }
                var lastPlayer = globalV.Players.LastOrDefault(p => p.TurnOrder == globalV.Players.Max(pl => pl.TurnOrder));
                if(lastPlayer==null){
                    return;
                }
                var adjustedBetAm = Math.Min(10, lastPlayer.Chips);
                await _gameRepository.PlaceBet(lastPlayer.Id, globalV.Id, 10, adjustedBetAm);
                await _gameRepository.ChangeChips(lastPlayer.Id, adjustedBetAm*-1);
                await _gameRepository.ChangePot(adjustedBetAm, globalV.Id);
            }
        }

        public async Task<string> KickPlayerAsync(string username, string kickedUsername)
        {
            if(username==kickedUsername){
                return null;
            }
            var player = await _lobbyRepository.GetPlayer(username);
            if(player==null){
                return null;
            }
            if (!player.IsAdmin)
            {
                return null;
            }
            if(kickedUsername.StartsWith("BotLeave")){
                return null;
            }
            var kickedPlayer = await _lobbyRepository.GetPlayer(kickedUsername);
            if(kickedPlayer==null){
                var kickedWaitingPlayer = await _lobbyRepository.GetWaitingRoomPlayer(kickedUsername);
                if(kickedWaitingPlayer==null){
                    return null;
                }
                if(kickedWaitingPlayer.GlobalVId!=player.GlobalVId){
                    return null;
                }
                await _lobbyRepository.RemoveWaitingRoomPlayer(kickedWaitingPlayer.Id);
                return "WaitingRoom";
            }
            if(kickedPlayer.GlobalVId!=player.GlobalVId){
                return null;
            }
            if (!player.Ready){
                await _lobbyRepository.DeletePlayer(kickedUsername);
                return "Lobby";
            }else{
                await _gameRepository.TurnPlayerIntoLeaveBot(kickedPlayer.Id);
                return $"BotLeave{kickedPlayer.Id}";
            }
        }
        public async Task<string> LeaveGameAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if(player==null){
                var waitingPlayer = await _lobbyRepository.GetWaitingRoomPlayer(username);
                if(waitingPlayer==null){
                    return null;
                }else{
                    await _lobbyRepository.RemoveWaitingRoomPlayer(waitingPlayer.Id);
                    return "WaitingRoom";
                }
            }
            var game = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (game != null)
            {
                if (!player.Ready){
                    if (game.Players.Count == 1){
                        await _lobbyRepository.DeletePlayer(username);
                        await _lobbyRepository.DeleteGame(game.Id);
                        return "All";
                    } 
                    else{
                        await _lobbyRepository.DeletePlayer(username);
                        await _lobbyRepository.ReassignAdmin(game.Id);
                        return "Lobby";
                    }
                }else{
                    bool isTurn = await _gameRepository.TurnPlayerIntoLeaveBot(player.Id);
                    await _lobbyRepository.ReassignAdmin(game.Id);
                    if(isTurn){
                        return $"BotLeave{player.Id}";
                    }
                }  
            }
            return null;
        }
        public async Task<IEnumerable<PlayerLobbyDTO>> GetPlayersAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if (player==null){
                return null;
            }
            var game = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (game == null)
            {
                return null;
            }

            var playerAdmins = game.Players.Select(player => new PlayerLobbyDTO 
            { 
                Username = player.Username, 
                IsAdmin = player.IsAdmin,
                IsReady = player.Ready
            }).ToList();

            return playerAdmins;
        }

        public async Task DealCardsAsync(string username, bool inGame)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if(player==null){
                return;
            }
            var game = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if ((player.IsAdmin&&!player.Ready)||inGame)
            {
                var players = game.Players;

                var deckCards = await _lobbyRepository.GetDeckCards(game.Id);

                var rng = new Random();

                foreach (var play in players)
                {
                    await _lobbyRepository.ClearPlayerCards(play.Id);
                    for (int i = 0; i < 2; i++)
                    {
                        var randomIndex = rng.Next(deckCards.Count);
                        var card = deckCards[randomIndex];
                        deckCards.RemoveAt(randomIndex);
                        await _lobbyRepository.RemoveDeckCard(card.Id);

                        var playerCard = new PlayerCard
                        {
                            CardNumber = card.CardNumber,
                            Suit = card.Suit,
                            PlayerId = play.Id
                        };
                        await _lobbyRepository.AddPlayerCard(playerCard);
                    }
                }
            }
        }
        public async Task AddDeckCardsAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if (player.IsAdmin&&player.Ready==false)
            {
                await _gameRepository.PopulateDeckCards(player.GlobalVId);
            }
        }
    }
}