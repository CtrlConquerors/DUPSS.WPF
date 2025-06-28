using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;

namespace DUPSS.API.Models.AccessLayer.DAOs
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
                TotalScore = assessmentResult.TotalScore,
                ScoreDetails = assessmentResult.ScoreDetails,
                Recommendation = assessmentResult.Recommendation,
                CompletedOn = assessmentResult.CompletedOn
            };
        }

        public async Task<AssessmentResultDTO?> GetByIdAsync(string resultId)
        {
            return await _context.AssessmentResult 
                .Include(ar => ar.Assessment)
                .Include(ar => ar.Member)
                .Where(ar => ar.ResultId == resultId)
                .Select(ar => new AssessmentResultDTO
                {
                    ResultId = ar.ResultId,
                    AssessmentId = ar.AssessmentId,
                    MemberId = ar.MemberId,
                    TotalScore = ar.TotalScore,
                    ScoreDetails = ar.ScoreDetails,
                    Recommendation = ar.Recommendation,
                    CompletedOn = ar.CompletedOn,
                    Assessment = ar.Assessment != null ? new AssessmentDTO
                    {
                        AssessmentId = ar.Assessment.AssessmentId,
                        AssessmentType = ar.Assessment.AssessmentType,
                        Description = ar.Assessment.Description,
                        Version = ar.Assessment.Version,
                        Language = ar.Assessment.Language
                    } : null,
                    Member = ar.Member != null ? new UserDTO
                    {
                        UserId = ar.Member.UserId,
                        Username = ar.Member.Username,
                        DoB = ar.Member.DoB,
                        PhoneNumber = ar.Member.PhoneNumber,
                        Email = ar.Member.Email,
                        RoleId = ar.Member.RoleId,
                        // PasswordHash = string.Empty 
                    } : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<AssessmentResultDTO>> GetAllAsync()
        {
            return await _context.AssessmentResult 
                .Include(ar => ar.Assessment)
                .Include(ar => ar.Member)
                .Select(ar => new AssessmentResultDTO
                {
                    ResultId = ar.ResultId,
                    AssessmentId = ar.AssessmentId,
                    MemberId = ar.MemberId,
                    TotalScore = ar.TotalScore,
                    ScoreDetails = ar.ScoreDetails,
                    Recommendation = ar.Recommendation,
                    CompletedOn = ar.CompletedOn,
                    Assessment = ar.Assessment != null ? new AssessmentDTO
                    {
                        AssessmentId = ar.Assessment.AssessmentId,
                        AssessmentType = ar.Assessment.AssessmentType,
                        Description = ar.Assessment.Description,
                        Version = ar.Assessment.Version,
                        Language = ar.Assessment.Language
                    } : null,
                    Member = ar.Member != null ? new UserDTO
                    {
                        UserId = ar.Member.UserId,
                        Username = ar.Member.Username,
                        DoB = ar.Member.DoB,
                        PhoneNumber = ar.Member.PhoneNumber,
                        Email = ar.Member.Email,
                        RoleId = ar.Member.RoleId,
                        // PasswordHash = string.Empty 
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
            existingResult.TotalScore = assessmentResult.TotalScore;
            existingResult.ScoreDetails = assessmentResult.ScoreDetails;
            existingResult.Recommendation = assessmentResult.Recommendation;
            existingResult.CompletedOn = assessmentResult.CompletedOn;

            await _context.SaveChangesAsync();
            return new AssessmentResultDTO
            {
                ResultId = existingResult.ResultId,
                AssessmentId = existingResult.AssessmentId,
                MemberId = existingResult.MemberId,
                TotalScore = existingResult.TotalScore,
                ScoreDetails = existingResult.ScoreDetails,
                Recommendation = existingResult.Recommendation,
                CompletedOn = existingResult.CompletedOn,
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