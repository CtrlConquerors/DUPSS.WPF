namespace DUPSS.DTO.DTOs
{
    public class AssessmentSubmissionDTO
    {
        public required string MemberId { get; set; }
        public List<AnswerSubmissionDTO> Answers { get; set; } = new();
        public List<TextAnswerSubmissionDTO> TextAnswers { get; set; } = new(); // For Text answers
        public int EarlyTextScore { get; set; } // Score for first three Text questions
    }

    public class AnswerSubmissionDTO
    {
        public required string AnswerId { get; set; }
    }
    public class TextAnswerSubmissionDTO
    {
        public string QuestionId { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }
}