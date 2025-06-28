using System.ComponentModel.DataAnnotations;

namespace DUPSS.API.Models.DTOs
{
    public class UserDTO
    {
        public required string UserId { get; set; }
        public required string Username { get; set; }
        public DateOnly? DoB { get; set; }
        public string? PhoneNumber { get; set; }
        public required string Email { get; set; }
        public string? ImageUrl { get; set; }
        public required string RoleId { get; set; }
        public string? PasswordHash => null; // PasswordHash is not exposed in DTOs
        public RoleDTO? Role { get; set; }
        
        //test
        public string Password { get; set; } = string.Empty;
    }
}
