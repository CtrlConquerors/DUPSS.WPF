using DUPSS.DTO.DTOs;
using DUPSS.Objects;

namespace DUPSS.DAO.Interfaces
{
    public interface IBlogDAO
    {
        Task<BlogDTO> CreateAsync(Blog blog);
        Task<BlogDTO> GetByIdAsync(string blogId);
        Task<List<BlogDTO>> GetAllAsync();
        Task<BlogDTO> UpdateAsync(Blog blog);
        Task<bool> DeleteAsync(string blogId);
    }
}
