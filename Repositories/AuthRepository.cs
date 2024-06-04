using pokerapi.Interfaces;
using pokerapi.Models; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace pokerapi.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly PokerContext _context;

        public AuthRepository(PokerContext context)
        {
            _context = context;
        }

        public async Task<User> AddUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetUser(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            return user;
        }

        public async Task<Player> GetPlayer(string username)
        {
            var player = await _context.Players.FirstOrDefaultAsync(u => u.Username == username);
            return player;
        }
    }
}