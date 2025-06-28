using DUPSS.API.Models.DTOs; // Thêm dòng này để sử dụng DTOs


namespace DUPSS.Web.Components.Service
{

    public class CampaignApiService : GenericApiService<CampaignDTO>
    {
        private readonly HttpClient _httpClient;

        public CampaignApiService(HttpClient httpClient)
            : base(httpClient, "api/Campaigns")
        {
            _httpClient = httpClient;
        }

        public async Task<CampaignDTO?> GetByIdAsync(string campaignId)
        {
            return await _httpClient.GetFromJsonAsync<CampaignDTO>($"api/Campaigns/GetById/{campaignId}");
        }
        public async Task<List<CampaignDTO>> GetCampaignsByUserIdAsync(string userId)
        {
            var url = $"api/CampaignRegistration/user/{userId}/campaigns";
  

            var result = await _httpClient.GetFromJsonAsync<List<CampaignDTO>>(url);
            return result ?? new List<CampaignDTO>();
        }

        public async Task<int> GetCountAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<int>("api/Campaigns/Count");
            return result;
        }
    }
}
