using BCrypt.Net;
using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;

namespace DUPSS.API.Models.AccessLayer.DAOs
{
    public class UserDAO : IUserDAO
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public UserDAO(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<UserDTO> CreateAsync(User user, string password)
        {
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.RoleId))
                throw new ArgumentException("Email, username, and role ID are required.");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password is required.");

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                // Check if email or username already exists
                if (await context.User.AnyAsync(u => u.Email == user.Email))
                    throw new Exception("A user with this email already exists.");
                if (await context.User.AnyAsync(u => u.Username == user.Username))
                    throw new Exception("A user with this username already exists.");

                // Generate UserId if not provided (e.g., using Guid)
                if (string.IsNullOrEmpty(user.UserId))
                    user.UserId = Guid.NewGuid().ToString();

                // Hash the password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                Console.WriteLine($"Hashed password for user {user.Username}: {user.PasswordHash}");

                // Validate role
                var role = await context.Role.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);
                if (role == null)
                    throw new Exception($"Role with ID {user.RoleId} does not exist.");

                context.User.Add(user);
                await context.SaveChangesAsync();

                return new UserDTO
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    DoB = user.DoB,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    ImageUrl = $"images/{user.UserId}.jpg",
                    RoleId = user.RoleId,
                    Role = new RoleDTO
                    {
                        RoleId = role.RoleId,
                        RoleName = role.RoleName
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"API: Failed to create user: {ex.Message}", ex);
            }
        }
        public async Task<int> CountAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.User.CountAsync();
        }

        public async Task<UserDTO> GetByIdAsync(string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.User
                .Where(u => u.UserId == userId)
                .Select(u => new UserDTO
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    DoB = u.DoB,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    ImageUrl = $"images/{u.UserId}.jpg",
                    RoleId = u.RoleId,
                    Role = u.Role != null ? new RoleDTO
                    {
                        RoleId = u.Role.RoleId,
                        RoleName = u.Role.RoleName
                    } : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<UserDTO>> GetAllAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.User
                .Select(u => new UserDTO
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    DoB = u.DoB,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    ImageUrl = $"images/{u.UserId}.jpg",
                    RoleId = u.RoleId,
                    Role = u.Role != null ? new RoleDTO
                    {
                        RoleId = u.Role.RoleId,
                        RoleName = u.Role.RoleName
                    } : null
                })
                .ToListAsync();
        }

        public async Task<UserDTO> UpdateAsync(User user)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var existingUser = await context.User.FindAsync(user.UserId);
            if (existingUser == null)
                throw new Exception($"User with ID {user.UserId} not found.");

            // Validate role
            var role = await context.Role.FirstOrDefaultAsync(r => r.RoleId == user.RoleId);
            if (role == null)
                throw new Exception($"Role with ID {user.RoleId} does not exist.");

            // Update fields (excluding PasswordHash, which requires a separate endpoint)
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.DoB = user.DoB;          
            existingUser.RoleId = user.RoleId;

            await context.SaveChangesAsync();
            return new UserDTO
            {
                UserId = existingUser.UserId,
                Username = existingUser.Username,
                DoB = existingUser.DoB,
                PhoneNumber = existingUser.PhoneNumber,
                Email = existingUser.Email,
                ImageUrl = $"images/{existingUser.UserId}.jpg",
                RoleId = existingUser.UserId,
                Role = new RoleDTO
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName
                }
            };
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.User.FindAsync(userId);
            if (user == null)
                return false;

            try
            {
                context.User.Remove(user);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete user: {ex.Message}", ex);
            }
        }

        
    }
}
