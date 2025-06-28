using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.AspNetCore.Mvc;

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class BlogTopicsController : ControllerBase
    {
        private readonly BlogTopicDAO _blogTopicDAO;

        public BlogTopicsController(AppDbContext context)
        {
            _blogTopicDAO = new BlogTopicDAO(context);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<BlogTopicDTO>>> GetAll()
        {
            try
            {
                var topics = await _blogTopicDAO.GetAllAsync();
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
        public async Task<ActionResult<BlogTopicDTO>> GetById(string topicId)
        {
            try
            {
                var topic = await _blogTopicDAO.GetByIdAsync(topicId);
                if (topic == null)
                    return NotFound($"BlogTopic with ID {topicId} not found.");
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
        public async Task<ActionResult<BlogTopicDTO>> Create([FromBody] BlogTopic blogTopic)
        {
            try
            {
                var createdTopic = await _blogTopicDAO.CreateAsync(blogTopic);
                return CreatedAtAction(nameof(GetById), new { topicId = createdTopic.BlogTopicId }, createdTopic);
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
        public async Task<ActionResult<BlogTopicDTO>> Update([FromBody] BlogTopic blogTopic)
        {
            try
            {
                var updatedTopic = await _blogTopicDAO.UpdateAsync(blogTopic);
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
                var result = await _blogTopicDAO.DeleteAsync(topicId);
                if (!result)
                    return NotFound($"BlogTopic with ID {topicId} not found.");
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