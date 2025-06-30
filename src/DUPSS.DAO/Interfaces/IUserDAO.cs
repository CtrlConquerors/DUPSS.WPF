using DUPSS.DTO.DTOs;
using DUPSS.Objects;
namespace DUPSS.DAO.Interfaces
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
