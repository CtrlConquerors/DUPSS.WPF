using DUPSS.DTO.DTOs;
using DUPSS.Objects;
namespace DUPSS.DAO.Interfaces
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
