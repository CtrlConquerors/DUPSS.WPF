using DUPSS.API.Models.DTOs;
namespace DUPSS.Web.Components.Service
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
