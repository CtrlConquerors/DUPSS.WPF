using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.AspNetCore.Mvc;

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class AssessmentAnswersController : ControllerBase
    {
        private readonly IAssessmentAnswerDAO _assessmentAnswerDAO;

        public AssessmentAnswersController(AppDbContext context)
        {
            _assessmentAnswerDAO = new AssessmentAnswerDAO(context);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<AssessmentAnswerDTO>>> GetAll()
        {
            try
            {
                var answers = await _assessmentAnswerDAO.GetAllAsync();
                return Ok(answers);
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

        [HttpGet("GetById/{answerId}")]
        public async Task<ActionResult<AssessmentAnswerDTO>> GetById(string answerId)
        {
            try
            {
                var answer = await _assessmentAnswerDAO.GetByIdAsync(answerId);
                if (answer == null)
                    return NotFound($"AssessmentAnswer with ID {answerId} not found.");
                return Ok(answer);
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
        public async Task<ActionResult<AssessmentAnswerDTO>> Create([FromBody] AssessmentAnswerDTO answerDTO)
        {
            try
            {
                var answer = new AssessmentAnswer
                {
                    AnswerId = answerDTO.AnswerId,
                    QuestionId = answerDTO.QuestionId,
                    Answer = answerDTO.Answer,
                    AnswerDetails = answerDTO.AnswerDetails,
                    ScoreValue = answerDTO.ScoreValue,
                    ScoreDescription = answerDTO.ScoreDescription
                };

                var createdAnswer = await _assessmentAnswerDAO.CreateAsync(answer);
                return CreatedAtAction(nameof(GetById), new { answerId = createdAnswer.AnswerId }, createdAnswer);
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
        public async Task<ActionResult<AssessmentAnswerDTO>> Update([FromBody] AssessmentAnswerDTO answerDTO)
        {
            try
            {
                var answer = new AssessmentAnswer
                {
                    AnswerId = answerDTO.AnswerId,
                    QuestionId = answerDTO.QuestionId,
                    Answer = answerDTO.Answer,
                    AnswerDetails = answerDTO.AnswerDetails,
                    ScoreValue = answerDTO.ScoreValue,
                    ScoreDescription = answerDTO.ScoreDescription
                };

                var updatedAnswer = await _assessmentAnswerDAO.UpdateAsync(answer);
                if (updatedAnswer == null)
                    return NotFound($"AssessmentAnswer with ID {answerDTO.AnswerId} not found.");
                return Ok(updatedAnswer);
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

        [HttpDelete("Delete/{answerId}")]
        public async Task<ActionResult<bool>> Delete(string answerId)
        {
            try
            {
                var result = await _assessmentAnswerDAO.DeleteAsync(answerId);
                if (!result)
                    return NotFound($"AssessmentAnswer with ID {answerId} not found.");
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
