using pokerapi.Interfaces;
using pokerapi.Models; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace pokerapi.Repositories
{
    public class LobbyRepository : ILobbyRepository
    {
        private readonly PokerContext _context;

        public LobbyRepository(PokerContext context)
        {
            _context = context;
        }

        public async Task<Player> GetPlayer(string username)
        {
            return await _context.Players.FirstOrDefaultAsync(p => p.Username == username);
        }

        public async Task ReadyPlayers(int gameId)
        {
            var players = await _context.Players.Where(p => p.GlobalVId == gameId).ToListAsync();
            foreach (var player in players)
            {
                player.Ready = true;
                player.Status = true;
                player.Chips = 500;
                player.Score = 0;
                if(player.TurnOrder==1){
                    player.IsTurn = true;
                }else{
                    player.IsTurn = false;
                }
            }
            
            _context.UpdateRange(players);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePlayer(string username)
        {
            var deletedPlayer = await GetPlayer(username);
            if (deletedPlayer != null)
            {
                _context.Players.Remove(deletedPlayer);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<GlobalV?> GetGameByIdAsync(int gameId)
        {
            return await _context.GlobalVs.Include(g => g.Players).FirstOrDefaultAsync(g => g.Id == gameId);
        }
        public async Task DeleteGame(int gameId)
        {
            var deletedGame = await GetGameByIdAsync(gameId);
            if (deletedGame != null)
            {
                _context.GlobalVs.Remove(deletedGame);
                await _context.SaveChangesAsync();
            }
        }
         public async Task<List<DeckCard>> GetDeckCards(int gameId)
        {
            return await _context.DeckCards.Where(dc => dc.GlobalVId == gameId).ToListAsync();
        }
        public async Task AddDeckCard(DeckCard deckCard)
        {
            _context.DeckCards.Add(deckCard);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveDeckCard(int cardId)
        {
            var card = await _context.DeckCards.FindAsync(cardId);
            if (card != null)
            {
                _context.DeckCards.Remove(card);
                await _context.SaveChangesAsync();
            }
        }
        public async Task AddPlayerCard(PlayerCard playerCard)
        {
            _context.PlayerCards.Add(playerCard);
            await _context.SaveChangesAsync();
        }
        public async Task ClearPlayerCards(int gameId)
        {
            var playerCards = await _context.PlayerCards.Where(pc => _context.Players.Any(p => p.Id == pc.PlayerId && p.GlobalVId == gameId)).ToListAsync();
            if (playerCards != null)
            {
                _context.PlayerCards.RemoveRange(playerCards);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<List<CommCard>> GetCommCards(int gameId)
        {
            return await _context.CommCards.Where(cc => cc.GlobalVId == gameId).ToListAsync();
        }
        public async Task<List<PlayerCard>> GetPlayerCards(int playerId)
        {
            return await _context.PlayerCards.Where(pc => pc.PlayerId == playerId).ToListAsync();
        }
        
        public async Task InitializeBets(int gameId)
        {
            var players = await _context.Players.Where(p => p.GlobalVId == gameId).ToListAsync();
            foreach (var player in players)
            {
                var bet = new Bet
                {
                    PlayerId = player.Id,
                    GlobalVId = gameId,
                    CurrentAm = 0,
                    TotalAm = 0
                };
                _context.Bets.Add(bet);
                await _context.SaveChangesAsync();
            }
        }

        public async Task InitializeTurnOrder(int gameId)
        {
            var players = await _context.Players.Where(p => p.GlobalVId == gameId).ToListAsync();
            var randomizedPlayers = players.OrderBy(p => Guid.NewGuid()).ToList();
            var order = 1;
            foreach (var player in randomizedPlayers)
            {
                player.TurnOrder = order;
                _context.Players.Update(player);
                order++;
            }
            await _context.SaveChangesAsync();
        }
    }
}