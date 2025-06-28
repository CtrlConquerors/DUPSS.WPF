namespace DUPSS.API.Models.DTOs
{
    public class AssessmentSubmissionDTO
    {
        public required string MemberId { get; set; }
        public List<AnswerSubmissionDTO> Answers { get; set; } = new();
    }

    public class AnswerSubmissionDTO
    {
        public required string AnswerId { get; set; }
    }
}