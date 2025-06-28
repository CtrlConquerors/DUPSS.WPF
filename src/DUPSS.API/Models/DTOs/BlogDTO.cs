namespace DUPSS.API.Models.DTOs
{
    public class BlogDTO
    {
        public required string BlogId { get; set; }
        public required string StaffId { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required string Status { get; set; }
        public required string BlogTopicId { get; set; }
        public UserDTO? Staff { get; set; }
        public BlogTopicDTO? BlogTopic { get; set; }
        public string? ImageUrl { get; set; }
    }
}
