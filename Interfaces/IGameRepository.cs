using System.Threading.Tasks;
using pokerapi.Models;

namespace pokerapi.Interfaces{
    public interface IGameRepository
    {
        Task SetAllPlayersActive(int gameId);
        Task ChangeChips(int playerId, int amount);
        Task SetPlayerTurn(int playerId, bool turn);
        Task<int> IncrementTurns(int gameId);
        Task ResetTurnsAndIncrementRound(int gameId);
        Task ResetGameState(int gameId);
        Task ChangePot(int amount, int gameId);
        Task<DeckCard> GetRandomDeckCard(int gameId);
        Task RemoveDeckCard(int cardId);
        Task AddCardToCommunity(DeckCard card);
        Task DeleteAllCommCards(int gameId);
        Task DeleteAllDeckCards(int gameId);
        Task PopulateDeckCards(int gameId);
        Task<int> GetTotalPlayerBet(int playerName);
        Task DecreaseTotalBet(int playerName, int subtract);
        Task DeleteExtraBets(int gameId);
        Task ResetAllBets(int gameId);
        Task PlaceBet(int playerId, int gameId, int currentBet, int totalBet);
        Task<int> GetLatestGameBet(int gameId);
        Task<int> GetLatestPlayerBet(int playerId);
        Task UpdateScore(int playerId, decimal score);
        Task ResetAllScores(int gameId);
        Task ResetTurns(int gameId);
        Task DeactivatePlayer(int playerId);
        Task<bool> TurnPlayerIntoLeaveBot(int playerId);
        Task ChangeShowdown(int gameId, bool showdown);
        Task ShiftTurnOrder(int gameId);
    }
}