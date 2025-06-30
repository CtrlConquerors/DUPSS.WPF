using DUPSS.Objects;

namespace DUPSS.ApiClients
{
    public class CourseTopicApiService : GenericApiService<CourseTopic>
    {
        private readonly HttpClient _httpClient;

        public CourseTopicApiService(HttpClient httpClient)
            : base(httpClient, "api/CourseTopics")
        {
            _httpClient = httpClient;
        }
    }
}
