using BCrypt.Net;
using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.Common;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DUPSS.API.Services
{
    public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
    {
        public static User? user = new()
        {
            UserId = Guid.NewGuid().ToString(), // Generate a unique ID for the user
            Username = string.Empty,           // Placeholder value, will be set during registration
            Email = string.Empty,              // Placeholder value, will be set during registration
            PasswordHash = string.Empty,       // Placeholder value, will be set during registration
            RoleId = "ME"         // Assign a default role ID
        };
        public async Task<TokenResponseDTO?> LoginAsync(LoginRequest request)
        {
            var userTask = context.User.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (userTask.Result is null)
            {
                return null;
            }

            user = userTask.Result;
            if (!BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.PasswordHash, HashType.SHA512))
            {
                return null;
            }
            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDTO> CreateTokenResponse(User user)
        {
            return new TokenResponseDTO
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        public async Task<User?> RegisterAsync(UserDTO request)
        {
            if (await context.User.AnyAsync(u => u.Email == request.Email))
            {
                return null;
            }

            var hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password, HashType.SHA512);

            user.UserId = Guid.NewGuid().ToString(); // Generate a unique ID for the user
            user.Email = request.Email;
            user.Username = request.Username;
            user.PhoneNumber = request.PhoneNumber;
            user.DoB = request.DoB ?? DateOnly.FromDateTime(DateTime.Now); // Default to current date if DoB is null
            user.PasswordHash = hashedPassword;
            context.User.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public async Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user == null)
            {
                return null;
            }
            return await CreateTokenResponse(user);
        }

        private async Task<User?> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var user = await context.User.FindAsync(userId);
            if (user is null || user.refreshToken != refreshToken || user.refreshTokenExpiry < DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.refreshToken = refreshToken;
            user.refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.RoleId),
                new Claim(ClaimTypes.Name, user.Username)

            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("Auth:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("Auth:Issuer"),
                audience: configuration.GetValue<string>("Auth:Audience"),
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),   //change this to auto logout user after certain login time
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        }
        public async Task<ForgotPasswordResponse?> ForgotPasswordAsync(string email)
        {
            var user = await context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return null;


            var Token = GenerateRefreshToken();

            user.PasswordResetToken = Token;
            user.TokenExpiry = DateTime.UtcNow.AddMinutes(30);
            await context.SaveChangesAsync();

            return new ForgotPasswordResponse { Email = user.Email, Token = Token };
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.PasswordResetToken != token || user.TokenExpiry < DateTime.UtcNow)
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword, HashType.SHA512);
            user.PasswordResetToken = null;
            user.TokenExpiry = null;
            await context.SaveChangesAsync();
            return true;
        }
    }
}
