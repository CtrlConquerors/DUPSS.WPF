using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;

namespace DUPSS.API.Models.AccessLayer.Interfaces
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
