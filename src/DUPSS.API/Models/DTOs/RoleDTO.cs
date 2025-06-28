namespace DUPSS.API.Models.DTOs
{
    public class RoleDTO
    {
        public required string RoleId { get; set; }
        public required string RoleName { get; set; }
        // Exclude Users to avoid circular reference
    }
}
