namespace DUPSS.DTO.DTOs
{
    public class AssessmentDTO
    {
        public required string AssessmentId { get; set; }
        public required string AssessmentType { get; set; }
        public string? Description { get; set; }
        // Exclude Results to avoid potential cycles; fetch separately if needed
    }
}
