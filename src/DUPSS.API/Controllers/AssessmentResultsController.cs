using DUPSS.DB;
using DUPSS.DAO.DAOs;
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

        [HttpPost("Create")]
        public async Task<ActionResult<AssessmentResultDTO>> Create([FromBody] AssessmentResult assessmentResult)
        {
            try
            {
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
        public async Task<ActionResult<AssessmentResultDTO>> Update([FromBody] AssessmentResult assessmentResult)
        {
            try
            {
                var updatedResult = await _assessmentResultDAO.UpdateAsync(assessmentResult);
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
