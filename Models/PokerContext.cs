using Microsoft.EntityFrameworkCore;

namespace pokerapi.Models
{    
    public class PokerContext : DbContext
    {
        
        public PokerContext(DbContextOptions<PokerContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<GlobalV> GlobalVs { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<CommCard> CommCards { get; set; }
        public DbSet<DeckCard> DeckCards { get; set; }
        public DbSet<PlayerCard> PlayerCards { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}