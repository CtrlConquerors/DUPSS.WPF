using DUPSS.DTO.DTOs;
using DUPSS.Objects;

namespace DUPSS.DAO.Interfaces
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
