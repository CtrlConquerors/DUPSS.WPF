using DUPSS.DTO.DTOs;
namespace DUPSS.ApiClients
{
    public class AssessmentResultApiService : GenericApiService<AssessmentResultDTO>
    {
        private readonly HttpClient _httpClient;

        public AssessmentResultApiService(HttpClient httpClient)
            : base(httpClient, "api/AssessmentResults")
        {
            _httpClient = httpClient;
        }
        
    }
}
