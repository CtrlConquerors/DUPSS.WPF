using DUPSS.DB;
using DUPSS.DAO.DAOs;
using DUPSS.DAO.Interfaces;
using DUPSS.DTO.DTOs;
using DUPSS.Objects;
using Microsoft.AspNetCore.Mvc;

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class AssessmentResultsController : ControllerBase
    {
        private readonly AssessmentResultDAO _assessmentResultDAO;

        public AssessmentResultsController(AppDbContext context)
        {
            _assessmentResultDAO = new AssessmentResultDAO(context);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<AssessmentResultDTO>>> GetAll()
        {
            try
            {
                var results = await _assessmentResultDAO.GetAllAsync();
                return Ok(results);
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

        [HttpGet("GetById/{resultId}")]
        public async Task<ActionResult<AssessmentResultDTO>> GetById(string resultId)
        {
            try
            {
                var result = await _assessmentResultDAO.GetByIdAsync(resultId);
                if (result == null)
                    return NotFound($"AssessmentResult with ID {resultId} not found.");
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

        [HttpGet("ByMember/{memberId}")]
        public async Task<ActionResult<IEnumerable<AssessmentResultDTO>>> GetByMember(string memberId)
        {
            try
            {
                var results = await _assessmentResultDAO.GetByMemberIdAsync(memberId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult<AssessmentResultDTO>> Create([FromBody] AssessmentResultDTO assessmentResultDTO)
        {
            try
            {
                // Map DTO to model
                var assessmentResult = new AssessmentResult
                {
                    ResultId = assessmentResultDTO.ResultId,
                    AssessmentId = assessmentResultDTO.AssessmentId,
                    MemberId = assessmentResultDTO.MemberId,
                    TotalScore = assessmentResultDTO.TotalScore,
                    ScoreDetails = assessmentResultDTO.ScoreDetails,
                    Recommendation = assessmentResultDTO.Recommendation,
                    CompletedOn = DateOnly.FromDateTime(DateTime.Now) // Set completed date to now
                };

                var createdResult = await _assessmentResultDAO.CreateAsync(assessmentResult);
                return CreatedAtAction(nameof(GetById), new { resultId = createdResult.ResultId }, createdResult);
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
        public async Task<ActionResult<AssessmentResultDTO>> Update([FromBody] AssessmentResultDTO assessmentResultDTO)
        {
            try
            {
                // Map DTO to model
                var assessmentResult = new AssessmentResult
                {
                    ResultId = assessmentResultDTO.ResultId,
                    AssessmentId = assessmentResultDTO.AssessmentId,
                    MemberId = assessmentResultDTO.MemberId,
                    TotalScore = assessmentResultDTO.TotalScore,
                    ScoreDetails = assessmentResultDTO.ScoreDetails,
                    Recommendation = assessmentResultDTO.Recommendation,
                    CompletedOn = assessmentResultDTO.CompletedOn ?? DateOnly.FromDateTime(DateTime.Now) // Use provided date or set to now
                };

                var updatedResult = await _assessmentResultDAO.UpdateAsync(assessmentResult);
                if (updatedResult == null)
                    return NotFound($"AssessmentResult with ID {assessmentResultDTO.ResultId} not found.");
                return Ok(updatedResult);
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

        [HttpDelete("Delete/{resultId}")]
        public async Task<ActionResult<bool>> Delete(string resultId)
        {
            try
            {
                var result = await _assessmentResultDAO.DeleteAsync(resultId);
                if (!result)
                    return NotFound($"AssessmentResult with ID {resultId} not found.");
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
