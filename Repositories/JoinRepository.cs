using pokerapi.Interfaces;
using pokerapi.Models; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace pokerapi.Repositories
{
    public class JoinRepository : IJoinRepository
    {
        private readonly PokerContext _context;

        public JoinRepository(PokerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GlobalV>> GetAvailableGamesAsync()
        {
            return await _context.GlobalVs.Include(g => g.Players).ToListAsync();
        }

        public async Task<GlobalV> CreateGameAsync(string name)
        {
            var newGame = new GlobalV { Name = name, Round  = 1 };
            _context.GlobalVs.Add(newGame);
            await _context.SaveChangesAsync();
            return newGame;
        }

        public async Task<GlobalV?> GetGameByIdAsync(int gameId)
        {
            return await _context.GlobalVs.FindAsync(gameId);
        }
        public async Task AddPlayerToGameAsync(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
        } 
    }
}
