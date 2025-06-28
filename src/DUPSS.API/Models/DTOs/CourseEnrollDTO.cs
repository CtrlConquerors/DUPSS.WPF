namespace DUPSS.API.Models.DTOs
{
    public class CourseEnrollDTO
    {
        public required string EnrollId { get; set; }
        public required string MemberId { get; set; }
        public required string CourseId { get; set; }
        public required string Status { get; set; }
        public DateOnly EnrollDate { get; set; }
        public DateOnly? CompleteDate { get; set; }
        public UserDTO? Member { get; set; }
        public CourseDTO? Course { get; set; }
    }
}
