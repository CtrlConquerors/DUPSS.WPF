using DUPSS.Common;
using DUPSS.DTO.DTOs;
using DUPSS.Objects;

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
