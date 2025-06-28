using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DUPSS.API.Models.Objects
{
    public class User
    {
        public User()
        {
        }

        [Key]
        public required string UserId { get; set; }
        [Required, MaxLength(100)]
        public required string Username { get; set; }
        public DateOnly? DoB { get; set; }
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        [Required, MaxLength(100)]
        public required string Email { get; set; }
        [Required, MaxLength(255)]
        public required string PasswordHash { get; set; }
        [Required, MaxLength(255)]
        public required string RoleId { get; set; }
        public string? refreshToken { get; set; }
        public DateTime? refreshTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        [NotMapped]
        public string? ImageUrl { get; set; }
        [JsonIgnore]
        public Role? Role { get; set; }
        [JsonIgnore]
        public List<Appointment> MemberAppointments { get; set; } = new List<Appointment>();
        [JsonIgnore]
        public List<Appointment> ConsultantAppointments { get; set; } = new List<Appointment>();
        [JsonIgnore]
        public List<Campaign> Campaigns { get; set; } = new List<Campaign>();
        [JsonIgnore]
        public List<Course> Courses { get; set; } = new List<Course>();
        [JsonIgnore]
        public List<CourseEnroll> Enrollments { get; set; } = new List<CourseEnroll>();
        [JsonIgnore]
        public List<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();
        [JsonIgnore]
        public List<Blog> Blogs { get; set; } = new List<Blog>();
    }
}
