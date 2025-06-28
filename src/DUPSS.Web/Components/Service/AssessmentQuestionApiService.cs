using DUPSS.API.Models.DTOs;
namespace DUPSS.Web.Components.Service
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
