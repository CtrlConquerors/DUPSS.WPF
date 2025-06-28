using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DUPSS.API.Models.Objects
{
    public class Course
    {
        public Course()
        {
            // Default constructor for EF Core
        }

        [Key]
        public required string CourseId { get; set; }

        [Required]
        public required string TopicId { get; set; }

        [Required, MaxLength(200)]
        public required string CourseName { get; set; }

        [Required, MaxLength(50)]
        public required string CourseType { get; set; }

        [Required]
        public required string StaffId { get; set; }

        // Changed from 'required string' to 'string?' to allow null values from the database
        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public required string ConsultantId { get; set; }

        // Navigation properties
        [JsonIgnore]
        public CourseTopic? Topic { get; set; }
        [JsonIgnore]
        public User? Staff { get; set; }
        [JsonIgnore]
        public User? Consultant { get; set; }
        [JsonIgnore]
        public List<CourseEnroll> Enrollments { get; set; } = new List<CourseEnroll>();

        // [NotMapped] properties for UI display, not stored in DB
        [NotMapped]
        public string? ImageUrl { get; set; }
        [NotMapped]
        public string? ImageUrl2 { get; set; }

        [NotMapped]
        public DateTime CreatedDate { get; set; }

        [NotMapped]
        public string? Status { get; set; }

        [NotMapped]
        public int Inventory { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }
    }
}
