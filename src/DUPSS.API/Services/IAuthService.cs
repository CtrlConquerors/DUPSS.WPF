using DUPSS.API.Models.Common;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;

namespace DUPSS.API.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDTO request);
        Task<TokenResponseDTO?> LoginAsync(LoginRequest request);

        Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request);
        Task<ForgotPasswordResponse?> ForgotPasswordAsync(string? email);
        Task<bool> ResetPasswordAsync(string? email, string? token, string? newPassword);
    }
}
