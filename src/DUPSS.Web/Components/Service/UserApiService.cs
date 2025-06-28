using DUPSS.API.Models.Common;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects; 

namespace DUPSS.Web.Components.Service
{
    
    public class UserApiService : GenericApiService<UserDTO>
    {
        private readonly HttpClient _httpClient;

        public UserApiService(HttpClient httpClient)
            : base(httpClient, "api/Users")
        {
            _httpClient = httpClient;
        }

       
        public async Task<UserDTO?> CreateAsync(CreateUserRequest user)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/Users/Create", user);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDTO>();
            }
            else
            {
                Console.WriteLine($"Failed to create user. Status code: {response.StatusCode}");
                if (response.Content != null)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error content: {errorContent}");
                }
            }
            return null;
        }

     
        public async Task<UserDTO?> UpdateAsync(UserDTO entity) 
        {
            var user = new User
            {
                UserId = entity.UserId,
                Username = entity.Username,
                DoB = entity.DoB,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                RoleId = entity.RoleId,
                PasswordHash = entity.PasswordHash, // Preserve original PasswordHash
                Role = new Role
                {
                    RoleId = entity.Role?.RoleId ?? "ME",
                    RoleName = entity.Role?.RoleName ?? "Member"
                }
            };

            var response = await _httpClient.PutAsJsonAsync("api/Users/Update", entity); 
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDTO>(); 
            }
            else
            {
                Console.WriteLine($"Failed to update user. Status code: {response.StatusCode}");
                if (response.Content != null)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error content: {errorContent}");
                }
            }
            return null;
        }

        
        public new async Task<List<UserDTO>> GetAllAsync()
        {

            return await base.GetAllAsync();
        }
        public async Task<List<UserDTO>> GetConsultantsAsync()
        {
            var response = await _httpClient.GetAsync("api/Users/consultants");
            if (response.IsSuccessStatusCode)
            {
                var consultants = await response.Content.ReadFromJsonAsync<List<UserDTO>>();
                return consultants ?? new List<UserDTO>();
            }
            else
            {
                Console.WriteLine($"Failed to fetch consultants. Status code: {response.StatusCode}");
                return new List<UserDTO>();
            }
        }

        public async Task<int> GetCountAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<int>("api/Users/Count");
            return result;
        }
    }
}