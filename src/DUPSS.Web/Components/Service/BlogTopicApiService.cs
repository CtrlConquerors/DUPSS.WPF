using DUPSS.API.Models.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DUPSS.API.Models.Objects;

namespace DUPSS.Web.Components.Service
{
    public class BlogTopicApiService : GenericApiService<BlogTopic>
    {
        private const string BaseUrl = "api/BlogTopics";
        public BlogTopicApiService(HttpClient httpClient) : base(httpClient, BaseUrl) { }
    }
}

