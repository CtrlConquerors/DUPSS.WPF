using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DUPSS.API.Models.Objects
{
    public class AssessmentResult
    {
        [Key]
        public required string ResultId { get; set; }
        [Required]
        public required string AssessmentId { get; set; }
        [Required]
        public required string MemberId { get; set; }
        public int? TotalScore { get; set; }
        public string? ScoreDetails { get; set; } // For complex results
        public string? Recommendation { get; set; }
        public DateOnly? CompletedOn { get; set; }
        [JsonIgnore]
        public User? Member { get; set; }
        [JsonIgnore]
        public Assessment? Assessment { get; set; }
    }
}
