using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DUPSS.API.Models.Objects
{
    public class Appointment
    {
        [Key]
        public required string AppointmentId { get; set; }
        [Required]
        public required string MemberId { get; set; }
        [Required]
        public required string ConsultantId { get; set; }
        [Required]
        public DateTime AppointmentDate { get; set; }
        [Required, MaxLength(50)]
        public required string Status { get; set; }
        [MaxLength(100)]
        public required string Topic { get; set; }
        public string? Notes { get; set; }
        [JsonIgnore]
        public User? Member { get; set; }
        [JsonIgnore]
        public User? Consultant { get; set; }
    }
}
