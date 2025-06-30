using DUPSS.DTO.DTOs;
namespace DUPSS.ApiClients
{
    public class AssessmentQuestionApiService : GenericApiService<AssessmentQuestionDTO>
    {
        private readonly HttpClient _httpClient;

        public AssessmentQuestionApiService(HttpClient httpClient)
            : base(httpClient, "api/AssessmentQuestions")
        {
            _httpClient = httpClient;
        }
        
    }
}
