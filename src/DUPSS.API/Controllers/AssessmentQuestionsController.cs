using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.AspNetCore.Mvc;

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class AssessmentQuestionsController : ControllerBase
    {
        private readonly IAssessmentQuestionDAO _assessmentQuestionDAO;

        public AssessmentQuestionsController(AppDbContext context)
        {
            _assessmentQuestionDAO = new AssessmentQuestionDAO(context);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<AssessmentQuestionDTO>>> GetAll()
        {
            try
            {
                var questions = await _assessmentQuestionDAO.GetAllAsync();
                return Ok(questions);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetById/{questionId}")]
        public async Task<ActionResult<AssessmentQuestionDTO>> GetById(string questionId)
        {
            try
            {
                var question = await _assessmentQuestionDAO.GetByIdAsync(questionId);
                if (question == null)
                    return NotFound($"AssessmentQuestion with ID {questionId} not found.");
                return Ok(question);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult<AssessmentQuestionDTO>> Create([FromBody] AssessmentQuestionDTO questionDTO)
        {
            try
            {
                var question = new AssessmentQuestion
                {
                    QuestionId = questionDTO.QuestionId,
                    AssessmentId = questionDTO.AssessmentId,
                    Question = questionDTO.Question,
                    QuestionType = questionDTO.QuestionType
                };

                var createdQuestion = await _assessmentQuestionDAO.CreateAsync(question);
                return CreatedAtAction(nameof(GetById), new { questionId = createdQuestion.QuestionId }, createdQuestion);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("Update")]
        public async Task<ActionResult<AssessmentQuestionDTO>> Update([FromBody] AssessmentQuestionDTO questionDTO)
        {
            try
            {
                var question = new AssessmentQuestion
                {
                    QuestionId = questionDTO.QuestionId,
                    AssessmentId = questionDTO.AssessmentId,
                    Question = questionDTO.Question,
                    QuestionType = questionDTO.QuestionType
                };

                var updatedQuestion = await _assessmentQuestionDAO.UpdateAsync(question);
                if (updatedQuestion == null)
                    return NotFound($"AssessmentQuestion with ID {questionDTO.QuestionId} not found.");
                return Ok(updatedQuestion);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("Delete/{questionId}")]
        public async Task<ActionResult<bool>> Delete(string questionId)
        {
            try
            {
                var result = await _assessmentQuestionDAO.DeleteAsync(questionId);
                if (!result)
                    return NotFound($"AssessmentQuestion with ID {questionId} not found.");
                return Ok(result);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}