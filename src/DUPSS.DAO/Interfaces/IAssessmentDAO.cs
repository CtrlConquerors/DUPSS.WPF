using DUPSS.DTO.DTOs;
using DUPSS.Objects;

namespace DUPSS.DAO.Interfaces
{
    public interface IAssessmentDAO
    {
        Task<AssessmentDTO> CreateAsync(Assessment assessment);
        Task<AssessmentDTO> GetByIdAsync(string assessmentId);
        Task<List<AssessmentDTO>> GetAllAsync();
        Task<AssessmentDTO> UpdateAsync(Assessment assessment);
        Task<bool> DeleteAsync(string assessmentId);
    }
}
