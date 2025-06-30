using DUPSS.DTO.DTOs;
using DUPSS.Objects;
namespace DUPSS.DAO.Interfaces
{
    public interface IAssessmentAnswerDAO
    {
        Task<AssessmentAnswerDTO> CreateAsync(AssessmentAnswer assessmentAnswer);
        Task<AssessmentAnswerDTO?> GetByIdAsync(string answerId);
        Task<List<AssessmentAnswerDTO>> GetAllAsync();
        Task<AssessmentAnswerDTO> UpdateAsync(AssessmentAnswer assessmentAnswer);
        Task<bool> DeleteAsync(string answerId);
    }
}
