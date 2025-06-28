using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.AspNetCore.Mvc;

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CourseTopicsController : ControllerBase
    {
        private readonly CourseTopicDAO _courseTopicDAO;

        public CourseTopicsController(AppDbContext context)
        {
            _courseTopicDAO = new CourseTopicDAO(context);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<CourseTopicDTO>>> GetAll()
        {
            try
            {
                var topics = await _courseTopicDAO.GetAllAsync();
                return Ok(topics);
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

        [HttpGet("GetById/{topicId}")]
        public async Task<ActionResult<CourseTopicDTO>> GetById(string topicId)
        {
            try
            {
                var topic = await _courseTopicDAO.GetByIdAsync(topicId);
                if (topic == null)
                    return NotFound($"CourseTopic with ID {topicId} not found.");
                return Ok(topic);
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
        public async Task<ActionResult<CourseTopicDTO>> Create([FromBody] CourseTopic courseTopic)
        {
            try
            {
                var createdTopic = await _courseTopicDAO.CreateAsync(courseTopic);
                return CreatedAtAction(nameof(GetById), new { topicId = createdTopic.TopicId }, createdTopic);
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
        public async Task<ActionResult<CourseTopicDTO>> Update([FromBody] CourseTopic courseTopic)
        {
            try
            {
                var updatedTopic = await _courseTopicDAO.UpdateAsync(courseTopic);
                return Ok(updatedTopic);
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

        [HttpDelete("Delete/{topicId}")]
        public async Task<ActionResult<bool>> Delete(string topicId)
        {
            try
            {
                var result = await _courseTopicDAO.DeleteAsync(topicId);
                if (!result)
                    return NotFound($"CourseTopic with ID {topicId} not found.");
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
