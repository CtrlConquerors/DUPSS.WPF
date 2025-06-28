using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DUPSS.API.Models.Objects
{
    public class CampaignRegistration
    {
        [Key]
        public string RegistrationId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public required string MemberId { get; set; }

        [Required]
        public required string CampaignId { get; set; }


        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public Campaign? Campaign { get; set; }
    }
}
