using DUPSS.API.Models.DTOs;
using System.Buffers.Text;
namespace DUPSS.Web.Components.Service
{
    public class AssessmentApiService : GenericApiService<AssessmentDTO>
    {
        private readonly HttpClient _httpClient;

        public AssessmentApiService(HttpClient httpClient)
            : base(httpClient, "api/Assessments")
        {
            _httpClient = httpClient;
        }
        public async Task<AssessmentResultDTO> SubmitAssessmentAsync(string assessmentId, AssessmentSubmissionDTO submission)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/Assessments/{assessmentId}/submit", submission);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AssessmentResultDTO>();
        }
    }
}
