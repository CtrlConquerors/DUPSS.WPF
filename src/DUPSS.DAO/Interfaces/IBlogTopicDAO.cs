using DUPSS.DTO.DTOs;
using DUPSS.Objects;

namespace DUPSS.DAO.Interfaces
{
    public interface IBlogTopicDAO
    {
        Task<BlogTopicDTO> CreateAsync(BlogTopic blogTopic);
        Task<List<BlogTopicDTO>> GetAllAsync();
        Task<BlogTopicDTO?> GetByIdAsync(string blogTopicId);
        Task<BlogTopicDTO> UpdateAsync(BlogTopic blogTopic);
        Task<bool> DeleteAsync(string blogTopicId);
    }
}