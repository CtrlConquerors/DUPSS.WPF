using DUPSS.DTO.DTOs;
// using DUPSS.API.Models.Objects; // Không cần thiết nếu bạn chỉ sử dụng DTO ở đây

namespace DUPSS.ApiClients
{
    // Thay đổi kiểu generic từ Blog sang BlogDTO
    public class BlogApiService : GenericApiService<BlogDTO>
    {
        private readonly HttpClient _httpClient;

        public BlogApiService(HttpClient httpClient)
            : base(httpClient, "api/Blogs")
        {
            _httpClient = httpClient;
        }
    }
}
