namespace DUPSS.DTO.DTOs
{
    public class CourseTopicDTO
    {
        public required string TopicId { get; set; }
        public required string TopicName { get; set; }
        // Exclude Courses to avoid circular reference
    }
}
