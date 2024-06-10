using pokerapi.Interfaces;
using pokerapi.Models; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace pokerapi.Repositories{
    public class GameRepository : IGameRepository
    {
        private readonly PokerContext _context;

        public GameRepository(PokerContext context)
        {
            _context = context;
        }

        public async Task SetAllPlayersActive(int gameId)
        {
            var players = _context.Players.Where(p => p.GlobalVId == gameId);
            foreach (var player in players)
            {
                player.Status = true;
            }
            await _context.SaveChangesAsync();
        }

        public async Task ChangeChips(int playerId, int amount)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player != null)
            {
                player.Chips += amount;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetPlayerTurn(int playerId, bool turn)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player != null)
            {
                player.Turn = turn;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<int> IncrementTurns(int gameId)
        {
            var game = await _context.GlobalVs.FindAsync(gameId);
            if (game != null)
            {
                game.Turns += 1;
                await _context.SaveChangesAsync();
                return game.Turns;
            }
            return -1; // Return an error code or throw an exception
        }

        public async Task ResetTurnsAndIncrementRound(int gameId)
        {
            var game = await _context.GlobalVs.FindAsync(gameId);
            if (game != null)
            {
                game.Turns = 0;
                game.Round += 1;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ResetGameState(int gameId)
        {
            var game = await _context.GlobalVs.FindAsync(gameId);
            if (game != null)
            {
                game.Turns = 0;
                game.Round = 1;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ChangePot(int amount, int gameId)
        {
            var game = await _context.GlobalVs.FindAsync(gameId);
            if (game != null)
            {
                game.Pot += amount;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DeckCard> GetRandomDeckCard(int gameId)
        {
            var game = await _context.GlobalVs.Include(g => g.DeckCards).FirstOrDefaultAsync(g => g.Id == gameId);
            if (game != null)
            {
                var random = new Random();
                int index = random.Next(game.DeckCards.Count);
                return game.DeckCards.ElementAt(index);
            }
            return null; // Return null or throw an exception
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

        public async Task AddCardToCommunity(DeckCard card)
        {
            var commCard = new CommCard { CardNumber = card.CardNumber, Suit = card.Suit, GlobalVId = card.GlobalVId };
            _context.CommCards.Add(commCard);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllCommCards(int gameId)
        {
            var game = _context.GlobalVs.Include(g => g.CommCards).FirstOrDefault(g => g.Id == gameId);
            if (game != null)
            {
                _context.CommCards.RemoveRange(game.CommCards);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllDeckCards(int gameId)
        {
            var game = _context.GlobalVs.Include(g => g.DeckCards).FirstOrDefault(g => g.Id == gameId);
            if (game != null)
            {
                _context.DeckCards.RemoveRange(game.DeckCards);
                await _context.SaveChangesAsync();
            }
        }

        public async Task PopulateDeckCards(int gameId)
        {
            var game = await _context.GlobalVs.FindAsync(gameId);
            if (game != null)
            {
                for (int suit = 1; suit <= 4; suit++)
                {
                    for (int cardNumber = 1; cardNumber <= 13; cardNumber++)
                    {
                        var card = new DeckCard { CardNumber = cardNumber, Suit = suit, GlobalVId = gameId };
                        _context.DeckCards.Add(card);
                    }
                }
                await _context.SaveChangesAsync();
            }
        }
        public async Task<int> GetTotalPlayerBet(int playerId)
        {
            var bet = await _context.Bets
                .Where(b => b.PlayerId == playerId)
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();
            return bet != null ? bet.TotalAm : -1; // Return an error code or throw an exception
        }

        public async Task DecreaseTotalBet(int playerId, int subtract)
        {
            var player = await _context.Players.Include(p => p.Bets).FirstOrDefaultAsync(p => p.Id == playerId);
            if (player != null)
            {
                var bet = player.Bets.OrderByDescending(b => b.Id).FirstOrDefault();
                if (bet != null)
                {
                    bet.TotalAm -= subtract;
                    await _context.SaveChangesAsync();
                }
            }
        }
        public async Task DeleteExtraBets(int gameId)
        {
            var game = await _context.GlobalVs.Include(g => g.Players).ThenInclude(p => p.Bets).FirstOrDefaultAsync(g => g.Id == gameId);
            if (game != null)
            {
                foreach (var player in game.Players)
                {
                    var bets = player.Bets.OrderByDescending(b => b.Id).ToList();
                    if (bets.Count != 0)
                    {
                        bets.First().CurrentAm = 0;
                        var extraBets = bets.Skip(1).ToList();
                        _context.Bets.RemoveRange(extraBets);
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task ResetAllBets(int gameId)
        {
            var game = await _context.GlobalVs.Include(g => g.Players).ThenInclude(p => p.Bets).FirstOrDefaultAsync(g => g.Id == gameId);
            if (game != null)
            {
                foreach (var player in game.Players)
                {
                    foreach (var bet in player.Bets)
                    {
                        bet.TotalAm = 0;
                    }
                }
                await _context.SaveChangesAsync();
            }
        }
        public async Task PlaceBet(int playerId, int gameId, int currentBet, int totalBet)
        {
            var bet = new Bet
            {
                PlayerId = playerId,
                CurrentAm = currentBet,
                TotalAm = totalBet,
                GlobalVId = gameId
            };
            await _context.Bets.AddAsync(bet);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetLatestGameBet(int gameId)
        {
            var bet = await _context.Bets
                .Where(b => b.GlobalVId == gameId)
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();
            return bet != null ? bet.CurrentAm : -1;
        }

        public async Task<int> GetLatestPlayerBet(int playerId)
        {
            var bet = await _context.Bets
                .Where(b => b.PlayerId == playerId)
                .OrderByDescending(b => b.Id)
                .FirstOrDefaultAsync();
            return bet != null ? bet.CurrentAm : -1;
        }

        public async Task UpdateScore(int playerId, decimal score)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
            if (player != null)
            {
                player.Score = score;
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task ResetAllScores(int gameId)
        {
            var players = await _context.Players.Where(p => p.GlobalVId == gameId).ToListAsync();
            foreach (var player in players)
            {
                player.Score = 0;
            }
            await _context.SaveChangesAsync();
        }
        
        public async Task ResetTurns(int gameId)
        {
            var globalV = await _context.GlobalVs.FirstOrDefaultAsync(g => g.Id == gameId);
            if (globalV != null)
            {
                globalV.Turns = 0;
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task DeactivatePlayer(int playerId)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
            if (player != null)
            {
                player.Status = false;
                await _context.SaveChangesAsync();
            }
        }
        
        
    }
}


