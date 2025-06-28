using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
namespace DUPSS.API.Models.AccessLayer.Interfaces
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
