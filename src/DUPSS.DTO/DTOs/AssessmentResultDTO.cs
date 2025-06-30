namespace DUPSS.DTO.DTOs
{
    public class AssessmentResultDTO
    {
        public required string ResultId { get; set; }
        public required string AssessmentId { get; set; }
        public required string MemberId { get; set; }
        public int Score { get; set; }
        public string? Recommendation { get; set; }
        public AssessmentDTO? Assessment { get; set; }
        public UserDTO? Member { get; set; }
    }
}
