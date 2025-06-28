using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;

namespace DUPSS.API.Models.AccessLayer.Interfaces
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
