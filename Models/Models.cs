using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace pokerapi.Models
{
   public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = []; 
        public byte[] PasswordSalt { get; set; } = [];
    }

   public class Player
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } 
        public string Ready { get; set; } = string.Empty;
        public string Turn { get; set; } = string.Empty;
        public int Chips { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Score { get; set; }

        [ForeignKey("GlobalV")]
        public int GlobalVId { get; set; }

        public virtual ICollection<PlayerCard> PlayerCards { get; set; } = [];
        public virtual ICollection<BetTrack> Bets { get; set; } = [];

    }

    public class GlobalV
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Round { get; set; }
        public int Turns { get; set; }
        public int Pot { get; set; }
        public virtual ICollection<Player> Players { get; set; } = [];
        public virtual ICollection<CommCard> CommCards { get; set; } = [];
        public virtual ICollection<DeckCard> DeckCards { get; set; } = [];
    }

    public class BetTrack
    {
        [Key]
        public int Id { get; set; }
        // Replace Username with PlayerId
        public int Amount { get; set; }
        public int TotalAm { get; set; }

        [ForeignKey("Player")]
        public int PlayerId { get; set; }

        [ForeignKey("GlobalV")]
        public int GameId { get; set; }
    }


    public class CommCard
    {
        [Key]
        public int Id { get; set; }
        public int CardNumber { get; set; }
        public string Suit { get; set; } = string.Empty;
        
        [ForeignKey("GlobalV")]
        public int GameId { get; set; }
    }

    public class DeckCard
    {
        [Key]
        public int Id { get; set; }
        public int CardNumber { get; set; }
        public int Suit { get; set; }

        [ForeignKey("GlobalV")]
        public int GameId { get; set; }
    }

    public class PlayerCard
    {
        [Key]
        public int Id { get; set; } 
        public int CardNumber { get; set; }
        public string Suit { get; set; } = string.Empty;

        [ForeignKey("Player")]
        public int PlayerId { get; set; } 
    }

}

