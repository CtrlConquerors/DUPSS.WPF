using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
namespace DUPSS.API.Models.AccessLayer.Interfaces
{
    public interface IUserDAO
    {
        Task<UserDTO> CreateAsync(User user, string password);
        Task<UserDTO> GetByIdAsync(string userId);
        Task<List<UserDTO>> GetAllAsync();
        Task<UserDTO> UpdateAsync(User user);
        Task<bool> DeleteAsync(string userId);
    }
}
