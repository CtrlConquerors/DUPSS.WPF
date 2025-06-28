using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DUPSS.API.Models.Objects
{
    public class Blog
    {
        [Key]
        public required string BlogId { get; set; }
        [Required]
        public required string StaffId { get; set; }
        [Required, MaxLength(200)]
        public required string Title { get; set; }
        [Required]
        public required string Content { get; set; }
        [Required, MaxLength(50)]
        public required string Status { get; set; }
        [Required]
        public required string BlogTopicId { get; set; }
        [JsonIgnore]
        public User? Staff { get; set; }
        [JsonIgnore]
        public BlogTopic? BlogTopic { get; set; }
        [NotMapped]
        public string? ImageUrl { get; set; }
    }
}