using System.Linq;
using Microsoft.AspNetCore.Mvc;
using pokerapi.Interfaces;
using pokerapi.Models;
using pokerapi.Hubs;
using Microsoft.AspNetCore.SignalR;
using SQLitePCL;
using System.Collections.Concurrent;


namespace pokerapi.Services{
    public class GameService : IGameService
    {
        private readonly ILobbyRepository _lobbyRepository;
        private readonly IGameRepository _gameRepository;
        private static ConcurrentDictionary<string, SemaphoreSlim> _playerLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        private readonly LobbyService _lobbyService;
        private readonly WinService _winService;

        public GameService(ILobbyRepository lobbyRepository, IGameRepository gameRepository)
        {
            _lobbyRepository = lobbyRepository;
            _gameRepository = gameRepository;
            _lobbyService = new LobbyService(lobbyRepository, gameRepository);
            _winService = new WinService(lobbyRepository, gameRepository);
            
        }

        public async Task<Player> GetPlayerAsync(string username){
            return await _lobbyRepository.GetPlayer(username);
        }
        public async Task<IEnumerable<PlayerGameDTO>> GetAllPlayersAsync(string username)
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
                Status = player.Status,
                IsAdmin = player.IsAdmin
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
                Showdown = globalV.Showdown,
                CommCards = globalV.CommCards,
                LastMoveTime = globalV.LastMoveTime
            };

            return game;
        }
        public async Task<IEnumerable<PlayerCard>> GetPlayerCardsAsync(string username)
        {
            var player = await _lobbyRepository.GetPlayer(username);
            if (player==null){
                return null;
            }

            return player.PlayerCards;
        }
        public async Task<IEnumerable<PlayerCardDTO>> GetAllPlayerCardsAsync(int gameId)
        {
            var globalV = await _lobbyRepository.GetGameByIdAsync(gameId);
            if (globalV == null)
            {
                return null;
            }
            if(!globalV.Showdown){
                return null;
            }
            return globalV.Players.Where(player => player.TurnOrder <= globalV.Turns && player.Score > 0.01m).SelectMany(player => player.PlayerCards, (player, card) => new PlayerCardDTO
            {
                Username = player.Username,
                CardNumber = card.CardNumber,
                Suit = card.Suit
            });
        }
        public async Task<IEnumerable<GameAction>> CheckAsync(string username){
            var gameActions = new List<GameAction>();
            var playerLock = _playerLocks.GetOrAdd(username, _ => new SemaphoreSlim(1, 1));
            await playerLock.WaitAsync();
            try
            {
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
                if(globalV.Showdown){
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
                    var newActions = await StartShowdown(globalV.Id, activePlayers);
                    gameActions[1].ActionName = "StartShowdown";
                    gameActions.AddRange(newActions);
                    return gameActions;
                }

                GameAction turnChange = await ChangeTurn(globalV.Id);
                if(turnChange!=null){
                    gameActions.Add(turnChange);
                }
                return gameActions;
            }
            finally
            {
                playerLock.Release();
            }   
        }

        public async Task<IEnumerable<GameAction>> BetAsync(string username, int betAmount)
        {
            var gameActions = new List<GameAction>();
            var playerLock = _playerLocks.GetOrAdd(username, _ => new SemaphoreSlim(1, 1));
            await playerLock.WaitAsync();
            try
            {
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
                if(globalV.Showdown){
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
            finally
            {
                playerLock.Release();
            }   
        }

        public async Task<IEnumerable<GameAction>> FoldAsync(string username){
            var gameActions = new List<GameAction>();
            var playerLock = _playerLocks.GetOrAdd(username, _ => new SemaphoreSlim(1, 1));
            await playerLock.WaitAsync();
            try
            {
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
                if(globalV.Showdown){
                    return gameActions;
                }
                var currentBet = await _gameRepository.GetLatestGameBet(globalV.Id);
                if (currentBet==0)
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
                    var newActions = await StartShowdown(globalV.Id, activePlayers);
                    gameActions.Add(new GameAction{
                        ActionName = "StartShowdown"
                    });
                    gameActions.AddRange(newActions);
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
                    var newActions = await StartShowdown(globalV.Id, activePlayers);
                    gameActions[1].ActionName = "StartShowdown";
                    gameActions.AddRange(newActions);
                    return gameActions;
                }

                GameAction turnChange = await ChangeTurn(globalV.Id);
                if(turnChange!=null){
                    gameActions.Add(turnChange);
                }
                return gameActions;
            }
            finally
            {
                playerLock.Release();
            }
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
            var currentBet = await _gameRepository.GetLatestGameBet(globalV.Id);
            if((((currentBet==0)&&(activePlayersWithChips.Count==1))||(nextPlayer.Chips==0))&&!globalV.Showdown){
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

        private async Task<IEnumerable<GameAction>> StartShowdown(int gameId, int activePlayers)
        {
            var gameActions = new List<GameAction>();
            var globalV = await _lobbyRepository.GetGameByIdAsync(gameId);
            await _gameRepository.ChangeShowdown(gameId, true);
            if(activePlayers==1){
                var activePlayer = globalV.Players.FirstOrDefault(p => p.Status);
                await _gameRepository.UpdateScore(activePlayer.Id, 1);
            }else{
                await _winService.CalculateWinAsync(gameId);
            }
            var currPlayer = globalV.Players.FirstOrDefault(p => p.IsTurn);
            await _gameRepository.SetPlayerTurn(currPlayer.Id, false);
            await _gameRepository.ResetTurnsAndIncrementRound(gameId);
            globalV.Players = globalV.Players.OrderBy(p => p.TurnOrder).ToList();
            var firstPlayer = globalV.Players.FirstOrDefault(p => p.Status);
            await _gameRepository.SetPlayerTurn(firstPlayer.Id, true);
            if(firstPlayer.Username.StartsWith("Bot")){
                gameActions.Add(new GameAction{
                    ActionName = "BotTurn",
                    PlayerName = firstPlayer.Username
                });
            }
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
        private async Task<IEnumerable<GameAction>> HandleShowdown(string username, string actionName)
        {
            var gameActions = new List<GameAction>();
            var playerLock = _playerLocks.GetOrAdd(username, _ => new SemaphoreSlim(1, 1));
            await playerLock.WaitAsync();
            try
            {
                var player = await _lobbyRepository.GetPlayer(username);
                if (player == null || !player.IsTurn)
                {
                    return gameActions;
                }
                var globalV = await _lobbyRepository.GetGameByIdAsync(player.GlobalVId);
                if (globalV == null || !globalV.Showdown)
                {
                    return gameActions;
                }

                var currentTurns = await _gameRepository.IncrementTurns(player.GlobalVId);
                var activePlayers = globalV.Players.Count(p => p.Status);

                gameActions.Add(new GameAction{
                    ActionName = actionName,
                    PlayerName = username,
                });
                if (actionName == "ShowCards")
                {
                    gameActions[0].PlayerCards = await _lobbyRepository.GetPlayerCards(player.Id);
                }
                if (actionName == "MuckCards")
                {
                    await _gameRepository.UpdateScore(player.Id, 0.0001m * currentTurns);
                }

                if (currentTurns == activePlayers)
                {
                    var newActions = await ResetGame(globalV.Id);
                    gameActions.Add(new GameAction{
                        ActionName = "GameEnd"
                    });
                    gameActions.AddRange(newActions);
                    return gameActions;
                }

                GameAction turnChange = await ChangeTurn(globalV.Id);
                if(turnChange != null){
                    gameActions.Add(turnChange);
                }
                return gameActions;
            }
            finally
            {
                playerLock.Release();
            }   
        }

        public async Task<IEnumerable<GameAction>> ShowCardsAsync(string username)
        {
            return await HandleShowdown(username, "ShowCards");
        }

        public async Task<IEnumerable<GameAction>> MuckCardsAsync(string username)
        {
            return await HandleShowdown(username, "MuckCards");
        }

        private async Task<IEnumerable<GameAction>> ResetGame(int gameId)
        {
            var gameActions = new List<GameAction>();
            var globalV = await _lobbyRepository.GetGameByIdAsync(gameId);
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
            await _gameRepository.SetAllPlayersActive(gameId);
            var playersToRemove = new List<Player>();
            foreach(var player in globalV.Players){
                if(player.Username.StartsWith("BotLeave")){
                    playersToRemove.Add(player);
                }
            }

            foreach(var player in playersToRemove){
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

            await _gameRepository.ResetGameState(gameId);
            await _gameRepository.DeleteExtraBets(gameId);
            await _gameRepository.DeleteAllCommCards(gameId);
            await _gameRepository.ResetAllBets(gameId);
            await _gameRepository.ResetAllScores(gameId);
            await _gameRepository.DeleteAllDeckCards(gameId);
            await _gameRepository.ShiftTurnOrder(gameId);

            globalV = await _lobbyRepository.GetGameByIdAsync(gameId);
            foreach(var player in globalV.Players)
            {
                await _gameRepository.SetPlayerTurn(player.Id, false);
            }
            var firstPlayer = globalV.Players.FirstOrDefault(p => p.TurnOrder == 1);
            await _gameRepository.SetPlayerTurn(firstPlayer.Id, true);

            var lastPlayer = globalV.Players.LastOrDefault(p => p.TurnOrder == globalV.Players.Max(pl => pl.TurnOrder));
            var adjustedBetAm = Math.Min(10, lastPlayer.Chips);
            await _gameRepository.PlaceBet(lastPlayer.Id, gameId, 10, adjustedBetAm);
            await _gameRepository.ChangeChips(lastPlayer.Id, adjustedBetAm*-1);
            await _gameRepository.ChangePot(adjustedBetAm, gameId);

            await _gameRepository.PopulateDeckCards(gameId);
            await _lobbyRepository.ClearPlayerCards(gameId);
            await _lobbyService.DealCardsAsync(globalV.Players.ElementAt(1).Username, true);
            return gameActions;

        }   
        public async Task<string> UpdateGameTimeAsync(int gameId)
        {
            DateTime currentTime = DateTime.Now;
            var game = await _lobbyRepository.GetGameByIdAsync(gameId);
            if(game==null){
                return null;
            }
            await _gameRepository.UpdateGameTime(gameId, currentTime);
            var currentPlayer = game.Players.FirstOrDefault(p => p.IsTurn);
            return currentPlayer.Username;
        }
    }
}

                