using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;

namespace DUPSS.API.Models.AccessLayer.Interfaces
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