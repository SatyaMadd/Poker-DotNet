using System.Linq;
using Microsoft.AspNetCore.Mvc;
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
                IsTurn = player.IsTurn,
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
                BetHasOccurred = (await _gameRepository.GetLatestGameBet(player.GlobalVId))!=0,
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

        public async Task<IEnumerable<GameAction>> CheckAsync(string username){
            var gameActions = new List<GameAction>();
            var player = await _lobbyRepository.GetPlayer(username);
            if (player==null){
                return gameActions;
            }
            if(player.IsTurn==false){
                return gameActions;
            }
            var globalV = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (globalV == null)
            {
                return gameActions;
            }
            
            var currentTurns = await _gameRepository.IncrementTurns(player.GlobalVId);

            var activePlayers = globalV.Players.Count(p => p.Status);

            int betAmount = await PlaceBetAsync(player.Id, globalV.Id);
            
            gameActions.Add(new GameAction{
                ActionName = "Check",
                PlayerName = username,
                Bet = betAmount
            });

            if (currentTurns == activePlayers)
            {
                await AdvanceRound(globalV.Id);
                gameActions.Add(new GameAction{
                    ActionName = "RoundEnd"
                });
            }


            if (globalV.Round > 4)
            {
                var newActions = await ResetGame(globalV.Id, activePlayers);
                gameActions[1].ActionName = "GameEnd";
                gameActions.AddRange(newActions);
                return gameActions;
            }

            GameAction turnChange = await ChangeTurn(globalV.Id);
            if(turnChange!=null){
                gameActions.Add(turnChange);
            }
            return gameActions;
            
        }

        public async Task<IEnumerable<GameAction>> BetAsync(string username, int betAmount)
        {
            var gameActions = new List<GameAction>();
            var player = await _lobbyRepository.GetPlayer(username);
            if (player==null){
                return gameActions;
            }
            if(player.IsTurn==false||player.Chips==0){
                return gameActions;
            }
            var globalV = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (globalV == null)
            {
                return gameActions;
            }
            var activePlayersWithChips = globalV.Players.Where(p => p.Status && p.Chips > 0).ToList();
            if (activePlayersWithChips.Count == 1)
            {
                return gameActions;
            }
            var currentBet = await _gameRepository.GetLatestGameBet(globalV.Id);
            if (currentBet >= betAmount)
            {
                return gameActions;
            }
            await _gameRepository.ResetTurns(player.GlobalVId);
            await _gameRepository.IncrementTurns(player.GlobalVId);

            int amount = await PlaceBetAsync(player.Id, globalV.Id, betAmount);
            gameActions.Add(new GameAction{
                ActionName = "Bet",
                PlayerName = username,
                Bet = betAmount
            });

            GameAction turnChange = await ChangeTurn(globalV.Id);
            if(turnChange!=null){
                gameActions.Add(turnChange);
            }
            return gameActions;    
        }

        public async Task<IEnumerable<GameAction>> FoldAsync(string username){
            var gameActions = new List<GameAction>();
            var player = await _lobbyRepository.GetPlayer(username);
            if (player==null){
                return gameActions;
            }
            if(player.IsTurn==false||player.Chips==0){
                return gameActions;
            }
            var globalV = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            if (globalV == null)
            {
                return gameActions;
            }

            await _gameRepository.DeactivatePlayer(player.Id);
            
            gameActions.Add(new GameAction{
                ActionName = "Fold",
                PlayerName = username,
            });

            globalV = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
            
            var activePlayers = globalV.Players.Count(p => p.Status);

            if (activePlayers==1)
            {
                await ResetGame(globalV.Id, activePlayers);
                gameActions.Add(new GameAction{
                    ActionName = "GameEnd"
                });
                return gameActions;
            }
            else if (globalV.Turns == activePlayers)
            {
                await AdvanceRound(globalV.Id);
                gameActions.Add(new GameAction{
                    ActionName = "RoundEnd"
                });
            }

            if (globalV.Round > 4)
            {
                var newActions = await ResetGame(globalV.Id, activePlayers);
                gameActions[1].ActionName = "GameEnd";
                gameActions.AddRange(newActions);
                return gameActions;
            }

            GameAction turnChange = await ChangeTurn(globalV.Id);
            if(turnChange!=null){
                gameActions.Add(turnChange);
            }
            return gameActions;
            
        }

        private async Task<GameAction> ChangeTurn(int gameId)
        {
            var globalV = await _lobbyRepository.GetGameByIdAsync(gameId);
            if (globalV == null)
            {
                return null;
            }
            globalV.Players = globalV.Players.OrderBy(p => p.TurnOrder).ToList();
            int currentIndex = globalV.Players.ToList().FindIndex(p => p.IsTurn == true);
            var nextPlayer = globalV.Players.FirstOrDefault(p => p.IsTurn == true);
            if (nextPlayer == null)
            {
                return null;
            }
            await _gameRepository.SetPlayerTurn(nextPlayer.Id, false);
            
            for (int i = currentIndex + 1; i != currentIndex; i++)
            {
                if (i >= globalV.Players.Count)
                {
                    i = -1; 
                    continue;
                }
                if (globalV.Players.ToList()[i].Status)
                {
                    nextPlayer = globalV.Players.ToList()[i];
                    break;
                }
            }
            await _gameRepository.SetPlayerTurn(nextPlayer.Id, true);
            var activePlayersWithChips = globalV.Players.Where(p => p.Status && p.Chips > 0).ToList();
            if(nextPlayer.Chips==0){
                return new GameAction{
                    ActionName = "AutoCheck",
                    PlayerName = nextPlayer.Username
                };
            }else if(nextPlayer.Username.StartsWith("Bot")){
                return new GameAction{
                    ActionName = "BotTurn",
                    PlayerName = nextPlayer.Username
                };
            }
            return null;
        }

        private async Task<IEnumerable<GameAction>> ResetGame(int gameId, int activePlayers)
        {
            var gameActions = new List<GameAction>();
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
            
            await _gameRepository.SetAllPlayersActive(gameId);
            foreach(var player in globalV.Players){
                if(player.Username.StartsWith("BotLeave")){
                    await _lobbyRepository.DeletePlayer(player.Username);
                    gameActions.Add(new GameAction{
                        ActionName = "RemoveBotLeave",
                        PlayerName = player.Username
                    });
                    globalV = await _lobbyRepository.GetGameByIdAsync(gameId);
                    if(globalV.Players.Count==1){
                        await _lobbyRepository.DeleteGame(gameId);
                            gameActions.Add(new GameAction{
                            ActionName = "DeleteGame"
                        });
                        return gameActions;
                    }
                }
            }
            await _gameRepository.ResetGameState(gameId);
            await _gameRepository.DeleteExtraBets(gameId);
            await _gameRepository.DeleteAllCommCards(gameId);
            await _gameRepository.ResetAllBets(gameId);
            await _gameRepository.ResetAllScores(gameId);
            await _gameRepository.DeleteAllDeckCards(gameId);

            await _gameRepository.PopulateDeckCards(gameId);
            await _lobbyRepository.ClearPlayerCards(gameId);
            await _lobbyService.DealCardsAsync(globalV.Players.ElementAt(1).Username, true);
            return gameActions;
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
        private async Task<int> PlaceBetAsync(int playerId, int gameId, int? betAmount = null)
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
            return adjustedBetAm;
        }

    }
}
