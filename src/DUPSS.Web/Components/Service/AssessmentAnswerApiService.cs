using DUPSS.API.Models.DTOs;
namespace DUPSS.Web.Components.Service
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
