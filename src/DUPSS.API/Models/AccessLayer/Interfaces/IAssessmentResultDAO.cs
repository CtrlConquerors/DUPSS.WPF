using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;

namespace DUPSS.API.Models.AccessLayer.Interfaces
{
    public interface IAssessmentResultDAO
    {
        Task<AssessmentResultDTO> CreateAsync(AssessmentResult assessmentResult);
        Task<AssessmentResultDTO?> GetByIdAsync(string resultId);
        Task<List<AssessmentResultDTO>> GetAllAsync();
        Task<AssessmentResultDTO> UpdateAsync(AssessmentResult assessmentResult);
        Task<bool> DeleteAsync(string resultId);
    }
}
