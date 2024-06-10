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
                await _lobbyRepository.ReadyPlayers(player.GlobalVId);
                await _lobbyRepository.InitializeTurnOrder(player.GlobalVId);
                await _lobbyRepository.InitializeBets(player.GlobalVId);
            }
        }

        public async Task KickPlayerAsync(string username, string kickedUsername)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if (player.IsAdmin)
            {
                await _lobbyRepository.DeletePlayer(kickedUsername);
            }
        }
        public async Task LeaveGameAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            var game = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (game != null)
            {
                if (game.Players.Count == 1){
                    await _lobbyRepository.DeletePlayer(username);
                    await _lobbyRepository.DeleteGame(game.Id);
                } 
                else{
                    await _lobbyRepository.DeletePlayer(username);
                }
            }          
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
                // Get all the players in the game
                var players = game.Players;

                // Get all the cards in the deck
                var deckCards = await _lobbyRepository.GetDeckCards(game.Id);

                // Random number generator
                var rng = new Random();

                foreach (var play in players)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        // Choose a random card from the deck
                        var randomIndex = rng.Next(deckCards.Count);
                        var card = deckCards[randomIndex];

                        // Remove the card from the deck
                        deckCards.RemoveAt(randomIndex);
                        await _lobbyRepository.RemoveDeckCard(card.Id);

                        // Add the card to the player's hand
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