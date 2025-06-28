using DUPSS.API.Models.Objects;
using System.ComponentModel.DataAnnotations;

namespace DUPSS.API.Models.DTOs
{
    public class AssessmentDTO
    {
        public required string AssessmentId { get; set; }
        public required string AssessmentType { get; set; }
        public required string Version { get; set; } = "1.0";
        public required string Language { get; set; } = "eng";
        public string? Description { get; set; }
        // Exclude Results to avoid potential cycles; fetch separately if needed
        // public List<AssessmentQuestion> Questions { get; set; } = new();
    }
}
