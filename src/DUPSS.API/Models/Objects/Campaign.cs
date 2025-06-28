using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Make sure this is included for [NotMapped]

namespace DUPSS.API.Models.Objects
{
    public class Campaign
    {
        [Key]
        public required string CampaignId { get; set; }
        [Required]
        public required string StaffId { get; set; }
        [Required, MaxLength(200)]
        public required string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        [Required, MaxLength(50)]
        public required string Status { get; set; }

        public string? Location { get; set; }
        public string? Introduction { get; set; }

        [JsonIgnore]
        public User? Staff { get; set; }



        public string? ImageUrl { get; set; }

        // New NotMapped property for Duration
        [NotMapped]
        public TimeSpan? Duration { get; set; }
    }
}
