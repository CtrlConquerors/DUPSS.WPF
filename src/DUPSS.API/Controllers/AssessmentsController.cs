using DUPSS.DB;
using DUPSS.DAO.DAOs;
using DUPSS.DTO.DTOs;
using DUPSS.Objects;
using Microsoft.AspNetCore.Mvc;

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class AssessmentsController : ControllerBase
    {
        private readonly AssessmentDAO _assessmentDAO;

        public AssessmentsController(AppDbContext context)
        {
            _assessmentDAO = new AssessmentDAO(context);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<AssessmentDTO>>> GetAll()
        {
            try
            {
                var assessments = await _assessmentDAO.GetAllAsync();
                return Ok(assessments);
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

        [HttpGet("GetById/{assessmentId}")]
        public async Task<ActionResult<AssessmentDTO>> GetById(string assessmentId)
        {
            try
            {
                var assessment = await _assessmentDAO.GetByIdAsync(assessmentId);
                if (assessment == null)
                    return NotFound($"Assessment with ID {assessmentId} not found.");
                return Ok(assessment);
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
        public async Task<ActionResult<AssessmentDTO>> Create([FromBody] Assessment assessment)
        {
            try
            {
                var createdAssessment = await _assessmentDAO.CreateAsync(assessment);
                return CreatedAtAction(nameof(GetById), new { assessmentId = createdAssessment.AssessmentId }, createdAssessment);
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
        public async Task<ActionResult<AssessmentDTO>> Update([FromBody] Assessment assessment)
        {
            try
            {
                var updatedAssessment = await _assessmentDAO.UpdateAsync(assessment);
                return Ok(updatedAssessment);
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

        [HttpDelete("Delete/{assessmentId}")]
        public async Task<ActionResult<bool>> Delete(string assessmentId)
        {
            try
            {
                var result = await _assessmentDAO.DeleteAsync(assessmentId);
                if (!result)
                    return NotFound($"Assessment with ID {assessmentId} not found.");
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
