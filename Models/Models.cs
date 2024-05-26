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
        public string Ready { get; set; } = string.Empty;
        public string Turn { get; set; } = string.Empty;
        public int Chips { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public int GameId { get; set; }

        [ForeignKey("GameId")]
        [JsonIgnore]
        public virtual GlobalV? Game { get; set; }

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
        public int PlayerId { get; set; }
        public int Amount { get; set; }
        public int TotalAm { get; set; }
        public int GameId { get; set; }

        [ForeignKey("GameId")]
        public virtual GlobalV? Game { get; set; }

        // Navigation property for the Player
        [ForeignKey("PlayerId")]
        public virtual Player? Player { get; set; }
    }


    public class CommCard
    {
        [Key]
        public int Id { get; set; }
        public int CardNumber { get; set; }
        public string Suit { get; set; } = string.Empty;
        public int GameId { get; set; }
        
        [ForeignKey("GameId")]
        public virtual GlobalV? Game { get; set; }
    }

    public class DeckCard
    {
        [Key]
        public int Id { get; set; }
        public int CardNumber { get; set; }
        public int Suit { get; set; }
        public int GameId { get; set; }

        [ForeignKey("GameId")]
        public virtual GlobalV? Game { get; set; }
    }

    public class PlayerCard
    {
        [Key]
        public int Id { get; set; } // This is now just 'Id' instead of 'PlayerCardId'
        public int PlayerId { get; set; } // This remains as a reference to the player's 'Id'
        public int CardNumber { get; set; }
        public string Suit { get; set; } = string.Empty;

        [ForeignKey("PlayerId")]
        public virtual Player? Player { get; set; }
    }

}

