using DUPSS.DTO.DTOs;
using DUPSS.Objects;

namespace DUPSS.DAO.Interfaces
{
    public interface IRoleDAO
    {
        Task<RoleDTO> CreateAsync(Role role);
        Task<RoleDTO> GetByIdAsync(string roleId);
        Task<List<RoleDTO>> GetAllAsync();
        Task<RoleDTO> UpdateAsync(Role role);
        Task<bool> DeleteAsync(string roleId);
    }
}
