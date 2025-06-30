using DUPSS.DTO.DTOs;
using DUPSS.Objects;
using DUPSS.DAO.Interfaces;
using Microsoft.EntityFrameworkCore;
using DUPSS.DB;
namespace DUPSS.DAO.DAOs
{
    public class AssessmentResultDAO : IAssessmentResultDAO
    {
        private readonly AppDbContext _context;

        public AssessmentResultDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AssessmentResultDTO> CreateAsync(AssessmentResult assessmentResult)
        {
            _context.AssessmentResult.Add(assessmentResult);
            await _context.SaveChangesAsync();
            return new AssessmentResultDTO
            {
                ResultId = assessmentResult.ResultId,
                AssessmentId = assessmentResult.AssessmentId,
                MemberId = assessmentResult.MemberId,
                //Score = assessmentResult.Score,
                Recommendation = assessmentResult.Recommendation
            };
        }

        public async Task<AssessmentResultDTO?> GetByIdAsync(string resultId)
        {
            return await _context.AssessmentResult
                .Where(ar => ar.ResultId == resultId)
                .Select(ar => new AssessmentResultDTO
                {
                    ResultId = ar.ResultId,
                    AssessmentId = ar.AssessmentId,
                    MemberId = ar.MemberId,
                    //Score = ar.Score,
                    Recommendation = ar.Recommendation,
                    Assessment = ar.Assessment != null ? new AssessmentDTO
                    {
                        AssessmentId = ar.Assessment.AssessmentId,
                        AssessmentType = ar.Assessment.AssessmentType,
                        Description = ar.Assessment.Description
                    } : null,
                    Member = ar.Member != null ? new UserDTO
                    {
                        UserId = ar.Member.UserId,
                        Username = ar.Member.Username,
                        DoB = ar.Member.DoB,
                        PhoneNumber = ar.Member.PhoneNumber,
                        Email = ar.Member.Email,
                        RoleId = ar.Member.RoleId
                    } : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<AssessmentResultDTO>> GetAllAsync()
        {
            return await _context.AssessmentResult
                .Select(ar => new AssessmentResultDTO
                {
                    ResultId = ar.ResultId,
                    AssessmentId = ar.AssessmentId,
                    MemberId = ar.MemberId,
                    //Score = ar.Score,
                    Recommendation = ar.Recommendation,
                    Assessment = ar.Assessment != null ? new AssessmentDTO
                    {
                        AssessmentId = ar.Assessment.AssessmentId,
                        AssessmentType = ar.Assessment.AssessmentType,
                        Description = ar.Assessment.Description
                    } : null,
                    Member = ar.Member != null ? new UserDTO
                    {
                        UserId = ar.Member.UserId,
                        Username = ar.Member.Username,
                        DoB = ar.Member.DoB,
                        PhoneNumber = ar.Member.PhoneNumber,
                        Email = ar.Member.Email,
                        RoleId = ar.Member.RoleId
                    } : null
                })
                .ToListAsync();
        }

        public async Task<AssessmentResultDTO> UpdateAsync(AssessmentResult assessmentResult)
        {
            var existingResult = await _context.AssessmentResult.FindAsync(assessmentResult.ResultId);
            if (existingResult == null)
                throw new Exception($"AssessmentResult with ID {assessmentResult.ResultId} not found.");

            existingResult.AssessmentId = assessmentResult.AssessmentId;
            existingResult.MemberId = assessmentResult.MemberId;
            //existingResult.Score = assessmentResult.Score;
            existingResult.Recommendation = assessmentResult.Recommendation;

            await _context.SaveChangesAsync();
            return new AssessmentResultDTO
            {
                ResultId = existingResult.ResultId,
                AssessmentId = existingResult.AssessmentId,
                MemberId = existingResult.MemberId,
                //Score = existingResult.Score,
                Recommendation = existingResult.Recommendation
            };
        }

        public async Task<bool> DeleteAsync(string resultId)
        {
            var assessmentResult = await _context.AssessmentResult.FindAsync(resultId);
            if (assessmentResult == null)
                return false;

            _context.AssessmentResult.Remove(assessmentResult);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
