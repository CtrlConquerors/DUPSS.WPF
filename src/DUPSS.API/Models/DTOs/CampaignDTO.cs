namespace DUPSS.API.Models.DTOs
{
    public class CampaignDTO
    {
        public required string CampaignId { get; set; }
        public required string StaffId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public required string Status { get; set; }

        public required string Location { get; set; }

        public required string Introduction { get; set; }

        public TimeSpan? Duration { get; set; } // NotMapped property included
        public UserDTO? Staff { get; set; }

        public string? ImageUrl { get; set; }
    }
}
