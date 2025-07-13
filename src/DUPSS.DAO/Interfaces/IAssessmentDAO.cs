using DUPSS.DTO.DTOs;
using DUPSS.Objects;

namespace DUPSS.DAO.Interfaces
{
    public interface IAssessmentDAO
    {
        Task<AssessmentDTO> CreateAsync(Assessment assessment);
        Task<Assessment> GetByIdAsync(string assessmentId, bool includeQuestions = false, bool includeAnswers = false);
        Task<List<AssessmentDTO>> GetAllAsync();
        Task<AssessmentDTO> UpdateAsync(Assessment assessment);
        Task<bool> DeleteAsync(string assessmentId);
    }
}
