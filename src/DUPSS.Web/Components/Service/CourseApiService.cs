using DUPSS.API.Models.DTOs; 
using DUPSS.Web.Components.Service; // Keep this for namespace


namespace DUPSS.Web.Components.Service
{
   
    public class CourseApiService : GenericApiService<CourseDTO>
    {
        private readonly HttpClient _httpClient;

        public CourseApiService(HttpClient httpClient)
            : base(httpClient, "api/Courses")
        {
            _httpClient = httpClient;
        }

        public async Task<int> GetCountAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<int>("api/Courses/Count");
            return result;
        }
    }
}
