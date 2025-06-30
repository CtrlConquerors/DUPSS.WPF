using DUPSS.DTO.DTOs;
using DUPSS.Objects;
namespace DUPSS.DAO.Interfaces
{
    public interface IAssessmentQuestionDAO
    {
        Task<AssessmentQuestionDTO> CreateAsync(AssessmentQuestion assessmentQuestion);
        Task<AssessmentQuestionDTO?> GetByIdAsync(string questionId);
        Task<List<AssessmentQuestionDTO>> GetAllAsync();
        Task<AssessmentQuestionDTO> UpdateAsync(AssessmentQuestion assessmentQuestion);
        Task<bool> DeleteAsync(string questionId);
    }
}
