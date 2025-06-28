using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
namespace DUPSS.API.Models.AccessLayer.Interfaces
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
