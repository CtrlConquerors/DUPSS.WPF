using DUPSS.API.Models.DTOs; 
using DUPSS.API.Models.Objects; // Still needed if 'Role' domain model is referenced elsewhere.

namespace DUPSS.Web.Components.Service
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
