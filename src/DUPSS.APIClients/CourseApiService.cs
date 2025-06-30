using DUPSS.DTO.DTOs;
using System.Net.Http.Json;

namespace DUPSS.ApiClients
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
