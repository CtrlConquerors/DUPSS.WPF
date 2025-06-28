using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;

namespace DUPSS.API.Models.AccessLayer.DAOs
{
    public class AssessmentQuestionDAO : IAssessmentQuestionDAO
    {
        private readonly AppDbContext _context;

        public AssessmentQuestionDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AssessmentQuestionDTO> CreateAsync(AssessmentQuestion assessmentQuestion)
        {
            _context.AssessmentQuestion.Add(assessmentQuestion);
            await _context.SaveChangesAsync();
            return new AssessmentQuestionDTO
            {
                QuestionId = assessmentQuestion.QuestionId,
                AssessmentId = assessmentQuestion.AssessmentId,
                Question = assessmentQuestion.Question,
                QuestionType = assessmentQuestion.QuestionType
            };
        }

        public async Task<AssessmentQuestionDTO?> GetByIdAsync(string questionId)
        {
            return await _context.AssessmentQuestion
                .Include(q => q.Assessment)
                .Include(q => q.Answers)
                .Where(q => q.QuestionId == questionId)
                .Select(q => new AssessmentQuestionDTO
                {
                    QuestionId = q.QuestionId,
                    AssessmentId = q.AssessmentId,
                    Question = q.Question,
                    QuestionType = q.QuestionType,
                    Assessment = q.Assessment != null ? new AssessmentDTO
                    {
                        AssessmentId = q.Assessment.AssessmentId,
                        AssessmentType = q.Assessment.AssessmentType,
                        Description = q.Assessment.Description,
                        Version = q.Assessment.Version,
                        Language = q.Assessment.Language
                    } : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<AssessmentQuestionDTO>> GetAllAsync()
        {
            return await _context.AssessmentQuestion
                .Include(q => q.Assessment)
                .Include(q => q.Answers)
                .Select(q => new AssessmentQuestionDTO
                {
                    QuestionId = q.QuestionId,
                    AssessmentId = q.AssessmentId,
                    Question = q.Question,
                    QuestionType = q.QuestionType,
                    Assessment = q.Assessment != null ? new AssessmentDTO
                    {
                        AssessmentId = q.Assessment.AssessmentId,
                        AssessmentType = q.Assessment.AssessmentType,
                        Description = q.Assessment.Description,
                        Version = q.Assessment.Version,
                        Language = q.Assessment.Language
                    } : null
                })
                .ToListAsync();
        }

        public async Task<AssessmentQuestionDTO> UpdateAsync(AssessmentQuestion assessmentQuestion)
        {
            var existingQuestion = await _context.AssessmentQuestion.FindAsync(assessmentQuestion.QuestionId);
            if (existingQuestion == null)
                throw new Exception($"AssessmentQuestion with ID {assessmentQuestion.QuestionId} not found.");

            existingQuestion.AssessmentId = assessmentQuestion.AssessmentId;
            existingQuestion.Question = assessmentQuestion.Question;
            existingQuestion.QuestionType = assessmentQuestion.QuestionType;

            await _context.SaveChangesAsync();
            return new AssessmentQuestionDTO
            {
                QuestionId = existingQuestion.QuestionId,
                AssessmentId = existingQuestion.AssessmentId,
                Question = existingQuestion.Question,
                QuestionType = existingQuestion.QuestionType
            };
        }

        public async Task<bool> DeleteAsync(string questionId)
        {
            var assessmentQuestion = await _context.AssessmentQuestion.FindAsync(questionId);
            if (assessmentQuestion == null)
                return false;

            _context.AssessmentQuestion.Remove(assessmentQuestion);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}