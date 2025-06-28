using System.ComponentModel.DataAnnotations;

namespace DUPSS.API.Models.Objects
{
    public class AssessmentQuestion
    {
        [Key]
        public required string QuestionId { get; set; }
        [Required, MaxLength(100)]
        public required string AssessmentId { get; set; }
        [Required, MaxLength(500)]
        public required string Question { get; set; }
        [Required, MaxLength(50)]
        public required string QuestionType { get; set; } = "MultipleChoice";
        public List<AssessmentAnswer> Answers { get; set; } = new();
        public Assessment? Assessment { get; set; }
    }
}
