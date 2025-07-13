using DUPSS.DTO.DTOs;
using System.Net.Http;
using System.Net.Http.Json;

namespace DUPSS.ApiClients
{
    public class CampaignRegistrationApiService
    {
        private readonly HttpClient _http;

        public CampaignRegistrationApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CampaignRegistrationDTO>> GetByUserIdAsync(string userId)
        {
            return await _http.GetFromJsonAsync<List<CampaignRegistrationDTO>>($"api/CampaignRegistration/user/{userId}")
                ?? new List<CampaignRegistrationDTO>();
        }
        public async Task<bool> UnregisterAsync(string memberId, string campaignId)
        {
            var response = await _http.DeleteAsync($"api/CampaignRegistration?memberId={memberId}&campaignId={campaignId}");
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> RegisterAsync(CampaignRegistrationDTO registration)
        {
            var response = await _http.PostAsJsonAsync("api/CampaignRegistration", registration);
            return response.IsSuccessStatusCode;
        }

    }

}
