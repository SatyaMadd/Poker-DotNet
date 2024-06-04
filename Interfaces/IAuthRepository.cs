using System.Threading.Tasks;
using pokerapi.Models;

namespace pokerapi.Interfaces
{
    public interface IAuthRepository
    {
        Task<User> AddUser(User user);
        Task<User> GetUser(string username);
        Task<Player> GetPlayer(string username);
    }
}