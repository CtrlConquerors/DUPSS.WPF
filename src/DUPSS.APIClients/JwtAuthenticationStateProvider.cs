// using Microsoft.AspNetCore.Components.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
// REMOVED: using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage; // This is Blazor-specific
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Linq; // Added for .Any() and .Select()
using DUPSS.Common; // CHANGED: Reference to your IProtectedLocalStorage moved to DUPSS.Common

namespace DUPSS.ApiClients
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly AuthApiService _authApiService;
        // CHANGED: Use your custom IProtectedLocalStorage interface
        private readonly IProtectedLocalStorage _localStorage;
        private const string AccessTokenKey = "accessToken";
        private string? _accessToken;
        private Timer? _logoutTimer;

        // CHANGED: Constructor now accepts IProtectedLocalStorage
        public JwtAuthenticationStateProvider(AuthApiService authApiService, IProtectedLocalStorage localStorage)
        {
            _authApiService = authApiService;
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                try
                {
                    // Use your custom _localStorage.GetAsync
                    var result = await _localStorage.GetAsync<string>(AccessTokenKey);
                    _accessToken = result.Success ? result.Value : null;

                    if (!string.IsNullOrEmpty(_accessToken))
                    {
                        SetLogoutTimer(_accessToken);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    // This catch block might be less relevant for WPF, but kept for consistency
                    // with the original Blazor context if it was copied.
                    Console.WriteLine($"Operation unavailable: {ex.Message}");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
                catch (Exception ex) // Catch general exceptions from storage service
                {
                    Console.WriteLine($"Error retrieving token from secure storage: {ex.Message}");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
            }
            if (string.IsNullOrEmpty(_accessToken))
            {
                Console.WriteLine("No access token, returning unauthenticated state");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = ParseClaimsFromJwt(_accessToken);
            if (!claims.Any())
            {
                Console.WriteLine("No claims parsed from JWT");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            Console.WriteLine($"Parsed claims: {string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}"))}");
            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationState(principal);
        }

        public async Task LoginAsync(string email, string password)
        {
            Console.WriteLine($"Calling AuthApiService.LoginAsync with email: {email}");
            var tokenResponse = await _authApiService.LoginAsync(email, password);
            if (tokenResponse == null)
            {
                Console.WriteLine("Login failed: TokenResponse is null");
                throw new Exception("Invalid email or password.");
            }
            _accessToken = tokenResponse.AccessToken;
            // Use your custom _localStorage.SetAsync
            await _localStorage.SetAsync(AccessTokenKey, _accessToken);
            SetLogoutTimer(_accessToken);
            Console.WriteLine("Login successful, notifying authentication state change");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Logout()
        {
            _accessToken = null;
            _logoutTimer?.Dispose();
            // Use your custom _localStorage.DeleteAsync
            await _localStorage.DeleteAsync(AccessTokenKey);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public string? GetAccessToken() => _accessToken;

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;
            return jsonToken?.Claims ?? Enumerable.Empty<Claim>();
        }

        private void SetLogoutTimer(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var expires = token.ValidTo;
            var timeToExpire = expires - DateTime.UtcNow;

            if (timeToExpire <= TimeSpan.Zero)
            {
                // This will trigger a Logout, which in turn calls _localStorage.DeleteAsync
                // and notifies authentication state change.
                Logout();
                return;
            }

            _logoutTimer = new Timer(_ =>
            {
                _logoutTimer?.Dispose();
                Logout();
            }, null, timeToExpire, Timeout.InfiniteTimeSpan);
        }
    }
}
