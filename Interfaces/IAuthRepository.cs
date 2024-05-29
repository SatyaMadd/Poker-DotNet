using System.Threading.Tasks;
using pokerapi.Models;

namespace pokerapi.Interfaces
{
    public interface IAuthRepository
    {
        Task<User> Register(User user);
        Task<User> Login(string username, string password);
        Task<bool> UserExists(string username);
    }
}