using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;

namespace DUPSS.API.Models.AccessLayer.Interfaces
{
    public interface ICourseDAO
    {
        Task<CourseDTO> CreateAsync(Course course);
        Task<CourseDTO> GetByIdAsync(string courseId);
        Task<List<CourseDTO>> GetAllAsync();
        Task<CourseDTO> UpdateAsync(Course course);
        Task<bool> DeleteAsync(string courseId);
    }
}
