using DUPSS.DTO.DTOs;
using DUPSS.Objects;

namespace DUPSS.ApiClients
{
   
    public class RoleApiService : GenericApiService<RoleDTO>
    {
        private readonly HttpClient _httpClient;

        public RoleApiService(HttpClient httpClient)
            : base(httpClient, "api/Roles")
        {
            _httpClient = httpClient;
        }

       
    }
}
