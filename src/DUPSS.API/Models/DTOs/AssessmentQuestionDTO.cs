using DUPSS.API.Models.Objects;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DUPSS.API.Models.DTOs
{
    public class AssessmentQuestionDTO
    {
        public required string QuestionId { get; set; }
        public required string AssessmentId { get; set; } = null!;
        public required string Question { get; set; }
        public required string QuestionType { get; set; } = "MultipleChoice";
        public List<AssessmentAnswerDTO> Answers { get; set; } = new();
        [JsonIgnore]
        public AssessmentDTO? Assessment { get; set; }
    }
}
