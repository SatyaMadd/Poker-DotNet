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
                player.Ready = "yes";
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
    }
}