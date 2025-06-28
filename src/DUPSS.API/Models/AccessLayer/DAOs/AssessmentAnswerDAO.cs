using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;

namespace DUPSS.API.Models.AccessLayer.DAOs
{
    public class AssessmentAnswerDAO : IAssessmentAnswerDAO
    {
        private readonly AppDbContext _context;

        public AssessmentAnswerDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AssessmentAnswerDTO> CreateAsync(AssessmentAnswer assessmentAnswer)
        {
            _context.AssessmentAnswer.Add(assessmentAnswer);
            await _context.SaveChangesAsync();
            return new AssessmentAnswerDTO
            {
                AnswerId = assessmentAnswer.AnswerId,
                QuestionId = assessmentAnswer.QuestionId,
                Answer = assessmentAnswer.Answer,
                AnswerDetails = assessmentAnswer.AnswerDetails,
                ScoreValue = assessmentAnswer.ScoreValue,
                ScoreDescription = assessmentAnswer.ScoreDescription
            };
        }

        public async Task<AssessmentAnswerDTO?> GetByIdAsync(string answerId)
        {
            return await _context.AssessmentAnswer
                .Include(a => a.Question)
                .Where(a => a.AnswerId == answerId)
                .Select(a => new AssessmentAnswerDTO
                {
                    AnswerId = a.AnswerId,
                    QuestionId = a.QuestionId,
                    Answer = a.Answer,
                    AnswerDetails = a.AnswerDetails,
                    ScoreValue = a.ScoreValue,
                    ScoreDescription = a.ScoreDescription,
                    Question = a.Question != null ? new AssessmentQuestionDTO
                    {
                        QuestionId = a.Question.QuestionId,
                        AssessmentId = a.Question.AssessmentId,
                        Question = a.Question.Question,
                        QuestionType = a.Question.QuestionType
                    } : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<AssessmentAnswerDTO>> GetAllAsync()
        {
            return await _context.AssessmentAnswer
                .Include(a => a.Question)
                .Select(a => new AssessmentAnswerDTO
                {
                    AnswerId = a.AnswerId,
                    QuestionId = a.QuestionId,
                    Answer = a.Answer,
                    AnswerDetails = a.AnswerDetails,
                    ScoreValue = a.ScoreValue,
                    ScoreDescription = a.ScoreDescription,
                    Question = a.Question != null ? new AssessmentQuestionDTO
                    {
                        QuestionId = a.Question.QuestionId,
                        AssessmentId = a.Question.AssessmentId,
                        Question = a.Question.Question,
                        QuestionType = a.Question.QuestionType
                    } : null
                })
                .ToListAsync();
        }

        public async Task<AssessmentAnswerDTO> UpdateAsync(AssessmentAnswer assessmentAnswer)
        {
            var existingAnswer = await _context.AssessmentAnswer.FindAsync(assessmentAnswer.AnswerId);
            if (existingAnswer == null)
                throw new Exception($"AssessmentAnswer with ID {assessmentAnswer.AnswerId} not found.");

            existingAnswer.QuestionId = assessmentAnswer.QuestionId;
            existingAnswer.Answer = assessmentAnswer.Answer;
            existingAnswer.AnswerDetails = assessmentAnswer.AnswerDetails;
            existingAnswer.ScoreValue = assessmentAnswer.ScoreValue;
            existingAnswer.ScoreDescription = assessmentAnswer.ScoreDescription;

            await _context.SaveChangesAsync();
            return new AssessmentAnswerDTO
            {
                AnswerId = existingAnswer.AnswerId,
                QuestionId = existingAnswer.QuestionId,
                Answer = existingAnswer.Answer,
                AnswerDetails = existingAnswer.AnswerDetails,
                ScoreValue = existingAnswer.ScoreValue,
                ScoreDescription = existingAnswer.ScoreDescription
            };
        }

        public async Task<bool> DeleteAsync(string answerId)
        {
            var assessmentAnswer = await _context.AssessmentAnswer.FindAsync(answerId);
            if (assessmentAnswer == null)
                return false;

            _context.AssessmentAnswer.Remove(assessmentAnswer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}