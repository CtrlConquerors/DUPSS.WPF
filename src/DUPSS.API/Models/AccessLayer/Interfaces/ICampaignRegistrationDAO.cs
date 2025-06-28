using DUPSS.API.Models.Objects;

namespace DUPSS.API.Models.AccessLayer.Interfaces
{
    public interface ICampaignRegistrationDAO
    {
        Task<IEnumerable<CampaignRegistration>> GetAllAsync();
        Task<IEnumerable<CampaignRegistration>> GetByUserIdAsync(string userId);
        Task<IEnumerable<CampaignRegistration>> GetByCampaignIdAsync(string campaignId);
        Task<CampaignRegistration?> GetByIdAsync(string registrationId);
        Task AddAsync(CampaignRegistration registration);
        Task DeleteAsync(string registrationId);
        Task<bool> ExistsAsync(string userId, string campaignId);

        Task<List<CampaignRegistration>> GetByUserIdWithCampaignAsync(string userId);
        Task<CampaignRegistration?> GetByMemberAndCampaignAsync(string memberId, string campaignId);

    }
}
