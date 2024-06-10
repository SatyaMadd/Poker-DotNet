using pokerapi.Models;
namespace pokerapi.Interfaces{
    public interface IWinService
    {
        Task CalculateWinAsync(int gameId);

    }
}
