using pokerapi.Interfaces;
using pokerapi.Models;
using SQLitePCL;


namespace pokerapi.Services{
    public class GameService : IGameService
    {
        private readonly ILobbyRepository _lobbyRepository;
        private readonly IGameRepository _gameRepository;
        
        private readonly LobbyService _lobbyService ;
        private readonly WinService _winService ;

        public GameService(ILobbyRepository lobbyRepository, IGameRepository gameRepository)
        {
            _lobbyRepository = lobbyRepository;
            _gameRepository = gameRepository;
            _lobbyService = new LobbyService(lobbyRepository, gameRepository);
            _winService = new WinService(lobbyRepository, gameRepository);
            
        }

        public async Task<IEnumerable<PlayerGameDTO>> GetPlayersAsync(string username)
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
            
            var players = game.Players.Select(player => new PlayerGameDTO 
            { 
                Username = player.Username, 
                Chips = player.Chips,
                Turn = player.Turn,
                Status = player.Status
            }).ToList();

            return players;
        }
        public async Task<GameDTO> GetGameAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if (player==null){
                return null;
            }
            var globalV = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (globalV == null)
            {
                return null;
            }
            
            var game =new GameDTO 
            { 
                Id = globalV.Id,
                Name = globalV.Name, 
                Pot = globalV.Pot, 
                Round = globalV.Round,
                CommCards = globalV.CommCards
            };

            return game;
        }
        public async Task<IEnumerable<PlayerCard>> GetCardsAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if (player==null){
                return null;
            }

            return player.PlayerCards;
        }

        public async Task CheckAsync(string username){
            var player = await _lobbyRepository.GetPlayer(username);
            if (player==null){
                return;
            }
            if(player.Turn==false){
                return;
            }
            var globalV = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (globalV == null)
            {
                return;
            }

            var currentTurns = await _gameRepository.IncrementTurns(player.GlobalVId);

            var activePlayers = globalV.Players.Count(p => p.Status);

            await PlaceBetAsync(player.Id, globalV.Id);

            if (currentTurns == activePlayers)
            {
               await AdvanceRound(globalV.Id);
            }


            if (globalV.Round > 4)
            {
                await ResetGame(globalV.Id, activePlayers);
            }

            await ChangeTurn(globalV.Id);
            
        }

        public async Task BetAsync(string username, int betAmount)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if (player==null){
                return;
            }
            if(player.Turn==false||player.Chips==0){
                return;
            }
            await _gameRepository.DeactivatePlayer(player.Id);
            var globalV = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (globalV == null)
            {
                return;
            }
            var activePlayersWithChips = globalV.Players.Where(p => p.Status && p.Chips > 0).ToList();
            if (activePlayersWithChips.Count == 1)
            {
                return;
            }
            var currentBet = await _gameRepository.GetLatestGameBet(globalV.Id);
            if (currentBet >= betAmount)
            {
                return;
            }
            await _gameRepository.ResetTurns(player.GlobalVId);
            await _gameRepository.IncrementTurns(player.GlobalVId);

            await PlaceBetAsync(player.Id, globalV.Id, betAmount);
            await ChangeTurn(globalV.Id);
        }

        public async Task FoldAsync(string username){
            var player = await _lobbyRepository.GetPlayer(username);
            if (player==null){
                return;
            }
            if(player.Turn==false||player.Chips==0){
                return;
            }
            await _gameRepository.DeactivatePlayer(player.Id);
            var globalV = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (globalV == null)
            {
                return;
            }
            var activePlayersWithChips = globalV.Players.Where(p => p.Status && p.Chips > 0).ToList();
            if (activePlayersWithChips.Count == 1)
            {
                return;
            }

            var activePlayers = globalV.Players.Count(p => p.Status);

            if (activePlayers==1)
            {
               await ResetGame(globalV.Id, activePlayers);
            }
            else if (globalV.Turns == activePlayers)
            {
               await AdvanceRound(globalV.Id);
            }

            if (globalV.Round > 4)
            {
                await ResetGame(globalV.Id, activePlayers);
            }

            await ChangeTurn(globalV.Id);
            
        }

        private async Task ChangeTurn(int gameId)
        {
            var globalV = await _lobbyRepository.GetGameByIdAsync(gameId);
            if (globalV == null)
            {
                return;
            }
            globalV.Players = globalV.Players.OrderBy(p => globalV.Order.ToList().IndexOf(p.Username)).ToList();
            int currentIndex = globalV.Players.ToList().FindIndex(p => p.Turn == true);
            var nextPlayer = globalV.Players.FirstOrDefault(p => p.Turn == true);
            if (nextPlayer == null)
            {
                return;
            }
            await _gameRepository.SetPlayerTurn(nextPlayer.Id, false);
            
            for (int i = currentIndex + 1; i != currentIndex; i++)
            {
                if (i >= globalV.Players.Count)
                {
                    i = -1; // it will become 0 at the start of the next loop iteration
                    continue;
                }

                // If the player is active, return this player
                if (globalV.Players.ToList()[i].Status)
                {
                    nextPlayer = globalV.Players.ToList()[i];
                    break;
                }
            }
            await _gameRepository.SetPlayerTurn(nextPlayer.Id, true);
        }

        private async Task ResetGame(int gameId, int activePlayers)
        {
            var globalV = await _lobbyRepository.GetGameByIdAsync(gameId);
            if(activePlayers==1){
                var activePlayer = globalV.Players.FirstOrDefault(p => p.Status);
                await _gameRepository.ChangeChips(activePlayer.Id, globalV.Pot);
                await _gameRepository.ChangePot(globalV.Pot*-1, gameId);
            }else{
                await _winService.CalculateWinAsync(gameId);
                globalV = await _lobbyRepository.GetGameByIdAsync(gameId);

                var winners = globalV.Players.OrderByDescending(player => player.Score);
                var players = globalV.Players;

                // Calculate winnings for each winner
                foreach (var winner in winners)
                {
                    var winnerBet = await _gameRepository.GetTotalPlayerBet(winner.Id);
                    var win = 0;

                    foreach (var anyPlayer in players)
                    {
                        var playerBet = await _gameRepository.GetTotalPlayerBet(anyPlayer.Id);
                        var subtract = Math.Min(winnerBet, playerBet);

                        await _gameRepository.DecreaseTotalBet(anyPlayer.Id, subtract);
                        await _gameRepository.ChangePot(subtract*-1, globalV.Id);

                        win += subtract;
                    }

                    await _gameRepository.ChangeChips(winner.Id, win);
                }
            }
            
            // Clean up
            await _gameRepository.SetAllPlayersActive(gameId);
            await _gameRepository.ResetGameState(gameId);
            await _gameRepository.DeleteExtraBets(gameId);
            await _gameRepository.DeleteAllCommCards(gameId);
            await _gameRepository.ResetAllBets(gameId);
            await _gameRepository.ResetAllScores(gameId);
            await _gameRepository.DeleteAllDeckCards(gameId);

            // Deal new cards
            await _gameRepository.PopulateDeckCards(gameId);
            await _lobbyService.DealCardsAsync(globalV.Players.ElementAt(1).Username, true);
        }
        
        private async Task AdvanceRound(int gameId)
        {
            await _gameRepository.ResetTurnsAndIncrementRound(gameId);
            await _gameRepository.DeleteExtraBets(gameId);
            
            var globalV = await _lobbyRepository.GetGameByIdAsync(gameId);
    
            var currentRound = globalV.Round;

            int cardsToDeal = currentRound == 2 ? 3 : currentRound <= 4 ? 1 : 0;

            for(int i = 1; i <= cardsToDeal; i++){
                var card =  await _gameRepository.GetRandomDeckCard(gameId);
                await _gameRepository.RemoveDeckCard(card.Id);
                await _gameRepository.AddCardToCommunity(card);
            }
        }
        private async Task PlaceBetAsync(int playerId, int gameId, int? betAmount = null)
        {
            var game = await _lobbyRepository.GetGameByIdAsync(gameId);
            var player = game.Players.FirstOrDefault(p => p.Id == playerId);
            var lastPlayerBet = await _gameRepository.GetLatestPlayerBet(playerId);
            var currChips = player.Chips;

            var currentGameBet = betAmount ?? await _gameRepository.GetLatestGameBet(gameId);
            var adjustedBetAm = Math.Min(currentGameBet - lastPlayerBet, currChips);   

            var totalPlayerBet = await _gameRepository.GetTotalPlayerBet(playerId);
            var updatedTotalPlayerBet = totalPlayerBet + adjustedBetAm;

            await _gameRepository.PlaceBet(playerId, gameId, currentGameBet, updatedTotalPlayerBet);
            await _gameRepository.ChangeChips(playerId, -adjustedBetAm);
            await _gameRepository.ChangePot(adjustedBetAm, gameId);
        }

    }
}
