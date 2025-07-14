using DUPSS.DTO.DTOs;
using System.Buffers.Text;
using System.Net.Http.Json;
namespace DUPSS.ApiClients
{
    public class AssessmentApiService : GenericApiService<AssessmentDTO>
    {
        private readonly HttpClient _httpClient;

        public AssessmentApiService(HttpClient httpClient)
            : base(httpClient, "api/Assessments")
        {
            _httpClient = httpClient;
        }
        public async Task<AssessmentResultDTO> SubmitAssessmentAsync(string assessmentId, AssessmentResultDTO submission)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/Assessments/{assessmentId}/submit", submission);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AssessmentResultDTO>();
        }
    }
}
