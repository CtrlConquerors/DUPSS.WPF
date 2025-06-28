namespace DUPSS.API.Models.DTOs
{
    public class CampaignRegistrationDTO
    {
        public string RegistrationId { get; set; } = default!;
        public string MemberId { get; set; } = default!;
        public string CampaignId { get; set; } = default!;
        public DateTime RegisteredAt { get; set; }

        public CampaignDTO? Campaign { get; set; }
    }
}
