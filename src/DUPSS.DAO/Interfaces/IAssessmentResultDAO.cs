using DUPSS.DTO.DTOs;
using DUPSS.Objects;

namespace DUPSS.DAO.Interfaces
{
    public interface IAssessmentResultDAO
    {
        Task<AssessmentResultDTO> CreateAsync(AssessmentResult assessmentResult);
        Task<AssessmentResultDTO> GetByIdAsync(string resultId);
        Task<List<AssessmentResultDTO>> GetAllAsync();
        Task<AssessmentResultDTO> UpdateAsync(AssessmentResult assessmentResult);
        Task<bool> DeleteAsync(string resultId);
    }
}
