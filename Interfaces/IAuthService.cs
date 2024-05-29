using System.Threading.Tasks;
using pokerapi.Models;

namespace pokerapi.Interfaces{
    public interface IAuthService
    {
        Task<string> Register(UserRegistrationModel userModel);
        Task<string> Login(UserLoginModel userModel);
        Task<bool> UserExists(string username);
    }
}