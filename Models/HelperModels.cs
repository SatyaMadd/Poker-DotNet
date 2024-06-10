namespace pokerapi.Models{
    public class PlayerLobbyDTO
    {
        public string Username { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsReady { get; set; }
    }

    public class PlayerGameDTO
    {
        public string Username { get; set; } = string.Empty;
        public int Chips { get; set; }
        public bool Turn { get; set; } 
        public bool Status { get; set; }
    }
    public class GameDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Round { get; set; }
        public int Pot { get; set; }
        public virtual ICollection<CommCard> CommCards { get; set; } = [];
    }
    public class Card
    {
        public int CardNumber { get; set; }
        public int Suit { get; set; }
    }
}