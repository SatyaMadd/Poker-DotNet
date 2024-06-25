using pokerapi.Models;
namespace pokerapi.Interfaces
{
    public interface IBotService
    {
        Task<IEnumerable<GameAction>> BotMove(string username);
    }
}
