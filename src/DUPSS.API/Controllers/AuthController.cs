using DUPSS.API.Models.Common;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using DUPSS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DUPSS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {

        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserDTO request)
        {
            var user = await authService.RegisterAsync(request);
            if (user is null)
            {
                return BadRequest("User already exists or registration failed");
            }

            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<TokenResponseDTO>> Login(LoginRequest request)
        {
            var result = await authService.LoginAsync(request);
            if (result is null)
            {
                return BadRequest("Invalid email or password");
            }

            return Ok(result);
        }

        [HttpPost("Refresh")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenRequestDTO request)
        {
            var result = await authService.RefreshTokenAsync(request);
            if (result is null || result.AccessToken is null || result.AccessToken is null)
            {
                return BadRequest("Invalid refresh token");
            }
            return Ok(result);
        }

        [HttpPost("ForgotPassword")]
        public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(ForgotPasswordRequest request)
        {
            var result = await authService.ForgotPasswordAsync(request.Email);
            if (result == null)
                return BadRequest("User not found");
            // Return token to frontend so you can forward to reset page
            return Ok(result);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var success = await authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            if (!success)
                return BadRequest("Invalid or expired token.");
            return Ok("Password has been reset.");
        }


        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndPoint()
        {
            return Ok("You are authenticated");
        }

    }
}
