using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using pokerapi.Models;

namespace pokerapi.Interfaces
{
    public interface ILobbyRepository
    {
        Task<Player> GetPlayer(string username);
        Task ReadyPlayers(int gameId);
        Task DeletePlayer(string username);
        Task<GlobalV?> GetGameByIdAsync(int gameId);
        Task DeleteGame(int gameId);
        Task AddDeckCard(DeckCard deckCard);
        Task<List<DeckCard>> GetDeckCards(int gameId);
        Task RemoveDeckCard(int cardId);
        Task AddPlayerCard(PlayerCard playerCard);
        Task ClearPlayerCards(int gameId);
        Task<List<CommCard>> GetCommCards(int gameId);
        Task<List<PlayerCard>> GetPlayerCards(int playerId);
        Task InitializeBets(int gameId);
        Task InitializeTurnOrder(int gameId);
        Task<WaitingRoomPlayer> GetWaitingRoomPlayer(string username);
        Task RemoveWaitingRoomPlayer(int playerId);
        Task<IEnumerable<WaitingRoomPlayer>> GetAllWaitingRoomPlayers(int gameId);
    }
}