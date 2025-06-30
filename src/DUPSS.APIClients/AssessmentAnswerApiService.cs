using DUPSS.DTO.DTOs;
namespace DUPSS.ApiClients
{
    public class AssessmentAnswerApiService : GenericApiService<AssessmentAnswerDTO>
    {
        private readonly HttpClient _httpClient;

        public AssessmentAnswerApiService(HttpClient httpClient)
            : base(httpClient, "api/AssessmentAnswers")
        {
            _httpClient = httpClient;
        }
        
    }
}
