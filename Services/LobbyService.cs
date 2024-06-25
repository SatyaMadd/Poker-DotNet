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
            }
        }

        public async Task KickPlayerAsync(string username, string kickedUsername)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            var kickedPlayer = await _lobbyRepository.GetPlayer(kickedUsername);
            if(player==null || kickedPlayer==null){
                return;
            }
            if (player.IsAdmin)
            {
                if (!player.Ready){
                    await _lobbyRepository.DeletePlayer(username);
                }else{
                    await _gameRepository.TurnPlayerIntoLeaveBot(kickedPlayer.Id);
                }
            }
        }
        public async Task<string> LeaveGameAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            var game = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (game != null)
            {
                if (!player.Ready){
                    if (game.Players.Count == 1){
                        await _lobbyRepository.DeletePlayer(username);
                        await _lobbyRepository.DeleteGame(game.Id);
                    } 
                    else{
                        await _lobbyRepository.DeletePlayer(username);
                    }
                }else{
                        bool isTurn = await _gameRepository.TurnPlayerIntoLeaveBot(player.Id);
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