using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace DUPSS.API.Models.Objects
{
    public class BlogTopic
    {
        [Key]
        public required string BlogTopicId { get; set; }
        [Required, MaxLength(100)]
        public required string BlogTopicName { get; set; }
        [JsonIgnore]
        public List<Blog> Blogs { get; set; } = new List<Blog>();
    }
}
