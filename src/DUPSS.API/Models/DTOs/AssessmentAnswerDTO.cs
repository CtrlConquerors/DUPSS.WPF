using DUPSS.API.Models.Objects;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DUPSS.API.Models.DTOs
{
    public class AssessmentAnswerDTO
    {
        public required string AnswerId { get; set; }
        public required string QuestionId { get; set; }
        public required string Answer { get; set; } 
        public string? AnswerDetails { get; set; }
        public int ScoreValue { get; set; } // Numeric score
        public string? ScoreDescription { get; set; } // Optional description

        [JsonIgnore]
        public AssessmentQuestionDTO? Question { get; set; }
    }
}
