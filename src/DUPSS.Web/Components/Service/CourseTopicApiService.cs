using DUPSS.API.Models.Objects;

namespace DUPSS.Web.Components.Service
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
