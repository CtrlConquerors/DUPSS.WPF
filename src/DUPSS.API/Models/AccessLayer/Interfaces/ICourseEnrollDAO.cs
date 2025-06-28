using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using System.Collections.Generic; // Required for List
using System.Threading.Tasks; // Required for Task

namespace DUPSS.API.Models.AccessLayer.Interfaces
{
    public interface ICourseEnrollDAO
    {
        Task<CourseEnrollDTO> CreateAsync(CourseEnroll courseEnroll);
        Task<CourseEnrollDTO?> GetByIdAsync(string enrollId); // Changed return type to nullable CourseEnrollDTO
        Task<List<CourseEnrollDTO>> GetAllAsync();
        Task<CourseEnrollDTO> UpdateAsync(CourseEnroll courseEnroll);
        Task<bool> DeleteAsync(string enrollId);
        // NEW: Add the method signature for GetEnrollmentsByMemberAndCourse
        Task<List<CourseEnrollDTO>?> GetEnrollmentsByMemberAndCourse(string memberId, string courseId);
    }
}
