using pokerapi.Interfaces;
using pokerapi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace pokerapi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository authRepo, IConfiguration config)
        {
            _authRepo = authRepo;
            _config = config;
        }

        public async Task<string> Register(UserRegistrationModel userModel)
        {
            if (await _authRepo.UserExists(userModel.Username))
            {
                throw new Exception("User already exists.");
            }

            CreatePasswordHash(userModel.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Username = userModel.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            var createdUser = await _authRepo.Register(user);

            var token = GenerateJwtToken(createdUser.Id.ToString(), createdUser.Username);
            return token;
        }

        public async Task<string> Login(UserLoginModel userModel)
        {
            var user = await _authRepo.Login(userModel.Username, userModel.Password);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            if (!VerifyPasswordHash(userModel.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new Exception("Password is incorrect.");
            }

            var token = GenerateJwtToken(user.Id.ToString(), user.Username);
            return token;
        }

        public async Task<bool> UserExists(string username)
        {
            return await _authRepo.UserExists(username);
        }

        private string GenerateJwtToken(string userId, string username)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]?? throw new ArgumentNullException("Jwt:Key")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expireDays = _config.GetValue<double>("Jwt:ExpireDays");
            var expires = DateTime.Now.AddDays(expireDays);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {   
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(storedHash);
            }
        }
    }
}