using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DUPSS.API.Models.Objects
{
    public class Role
    {
        [Key]
        public required string RoleId { get; set; }
        [Required, MaxLength(50)]
        public required string RoleName { get; set; }
        [JsonIgnore]
        public List<User> Users { get; set; } = new List<User>();
    }
}
