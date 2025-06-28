using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.Common;
using DUPSS.API.Models.DTOs; // Đảm bảo namespace này được bao gồm
using DUPSS.API.Models.Objects; // Đảm bảo namespace này được bao gồm cho User domain model
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DUPSS.API.Models.AccessLayer.Interfaces; // Thêm dòng này cho IUserDAO

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserDAO _userDAO; // Sử dụng interface cho DAO
        private readonly IConfiguration _configuration;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public UsersController(IUserDAO userDAO, IConfiguration configuration, IDbContextFactory<AppDbContext> contextFactory) // Inject interface
        {
            _userDAO = userDAO;
            _configuration = configuration;
            _contextFactory = contextFactory;
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] Models.Common.LoginRequest request)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();
                var user = await context.User
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    return Unauthorized("Invalid email or password.");

                // Generate JWT
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.RoleId ?? "ME")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(1),
                    signingCredentials: creds);

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            try
            {
                // userDAO.GetAllAsync() đã trả về List<UserDTO>
                var users = await _userDAO.GetAllAsync();
                return Ok(users);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetById/{userId}")]
        public async Task<ActionResult<UserDTO>> GetById(string userId)
        {
            try
            {
                // userDAO.GetByIdAsync() đã trả về UserDTO
                var user = await _userDAO.GetByIdAsync(userId);
                if (user == null)
                    return NotFound($"User with ID {userId} not found.");
                return Ok(user);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("Count")]
        public async Task<ActionResult<int>> GetCount()
        {
            try
            {
                // Replace _userDAO.CountAsync() with a manual count operation
                var users = await _userDAO.GetAllAsync();
                var count = users.Count;
                return Ok(count);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult<UserDTO>> Create([FromBody] CreateUserRequest request)
        {
            try
            {
                Console.WriteLine($"Creating user with username: {request.User.Username}, email: {request.User.Email}");

                // Ánh xạ UserDTO từ request thành User domain model cho DAO
                var userDomainModel = new User
                {
                    UserId = request.User.UserId,
                    Username = request.User.Username,
                    Email = request.User.Email,
                    PhoneNumber = request.User.PhoneNumber,
                    DoB = request.User.DoB,
                    RoleId = request.User.RoleId,
                    PasswordHash = request.User.PasswordHash // Hoặc để DAO xử lý hash
                };

                // Gọi DAO với User domain model
                var createdUser = await _userDAO.CreateAsync(userDomainModel, request.Password);
                return CreatedAtAction(nameof(GetById), new { userId = createdUser.UserId }, createdUser);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("Update")]
        public async Task<ActionResult<UserDTO>> Update([FromBody] UserDTO userDto)
        {
            try
            {
                // Ánh xạ UserDTO từ request thành User domain model cho DAO
                var userDomainModel = new User
                {
                    UserId = userDto.UserId,
                    Username = userDto.Username,
                    Email = userDto.Email,
                    PhoneNumber = userDto.PhoneNumber,
                    DoB = userDto.DoB,
                    RoleId = userDto.RoleId,
                    PasswordHash = userDto.PasswordHash // Giữ nguyên hash hiện có, không cập nhật password qua đây
                };

                // Gọi DAO với User domain model
                var updatedUser = await _userDAO.UpdateAsync(userDomainModel);
                return Ok(updatedUser);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("Delete/{userId}")]
        public async Task<ActionResult<bool>> Delete(string userId)
        {
            try
            {
                var result = await _userDAO.DeleteAsync(userId);
                if (!result)
                    return NotFound($"User with ID {userId} not found.");
                return Ok(result);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("consultants")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetConsultants()
        {
            try
            {
                var allUsers = await _userDAO.GetAllAsync();
                var consultants = allUsers
                    .Where(u => u.Role != null && u.Role.RoleName == "Consultant")
                    .ToList();

                return Ok(consultants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



    }
}
