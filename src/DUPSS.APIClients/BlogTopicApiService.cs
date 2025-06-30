using DUPSS.DTO.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DUPSS.Objects;

namespace DUPSS.ApiClients
{
    public class BlogTopicApiService : GenericApiService<BlogTopic>
    {
        private const string BaseUrl = "api/BlogTopics";
        public BlogTopicApiService(HttpClient httpClient) : base(httpClient, BaseUrl) { }
    }
}

