using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;

namespace DUPSS.API.Models.AccessLayer.Interfaces
{
    public interface ICourseTopicDAO
    {
        Task<CourseTopicDTO> CreateAsync(CourseTopic courseTopic);
        Task<CourseTopicDTO> GetByIdAsync(string topicId);
        Task<List<CourseTopicDTO>> GetAllAsync();
        Task<CourseTopicDTO> UpdateAsync(CourseTopic courseTopic);
        Task<bool> DeleteAsync(string topicId);
    }
}
