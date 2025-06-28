using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
namespace DUPSS.API.Models.AccessLayer.Interfaces
{
    public interface ICampaignDAO
    {
        Task<CampaignDTO> CreateAsync(Campaign campaign);
        Task<CampaignDTO> GetByIdAsync(string campaignId);
        Task<List<CampaignDTO>> GetAllAsync();
        Task<CampaignDTO> UpdateAsync(CampaignDTO campaign);
        Task<bool> DeleteAsync(string campaignId);
    }
}
