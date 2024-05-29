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
    }
}
