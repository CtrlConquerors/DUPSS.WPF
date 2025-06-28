using DUPSS.API.Models.Common;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;

namespace DUPSS.Web.Components.Service
{
    public class AuthApiService
    {
        private readonly HttpClient _httpClient;

        public AuthApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TokenResponseDTO> LoginAsync(string email, string password)
        {
            LoginRequest request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            Console.WriteLine($"Sending POST to api/Auth/Login with email: {email}");
            var response = await _httpClient.PostAsJsonAsync("api/Auth/Login", request);
            Console.WriteLine($"API response status: {response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponseDTO>();
                if (result == null)
                {
                    Console.WriteLine("Failed to deserialize TokenResponseDTO");
                    throw new InvalidOperationException("Failed to deserialize TokenResponseDTO");
                }
                return result;
            }
            else if(response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                    response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login failed with status {response.StatusCode}: {errorMessage}");
                throw new HttpRequestException($"Login failed: {errorMessage}");
            }
        }

        public async Task<User> RegisterAsync(UserDTO user)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/Register", user);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<User>() ?? throw new InvalidOperationException("Failed to deserialize User");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Register failed: {errorMessage}");
            }
        }

        public async Task<TokenResponseDTO> RefreshTokenAsync(RefreshTokenRequestDTO request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/Refresh", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TokenResponseDTO>() ?? throw new InvalidOperationException("Failed to deserialize TokenResponseDTO");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Refresh token failed: {errorMessage}");
            }
        }
        public async Task<ForgotPasswordResponse?> ForgotPasswordAsync(string email)
        {
            var request = new ForgotPasswordRequest { Email = email };
            var response = await _httpClient.PostAsJsonAsync("api/Auth/ForgotPassword", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
            }
            return null;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var request = new ResetPasswordRequest
            {
                Email = email,
                Token = token,
                NewPassword = newPassword
            };
            var response = await _httpClient.PostAsJsonAsync("api/Auth/ResetPassword", request);
            return response.IsSuccessStatusCode;
        }
    }
}
