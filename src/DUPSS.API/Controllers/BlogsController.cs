using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.AspNetCore.Mvc;

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class BlogsController : ControllerBase
    {
        private readonly BlogDAO _blogDAO;

        public BlogsController(AppDbContext context)
        {
            _blogDAO = new BlogDAO(context);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<BlogDTO>>> GetAll()
        {
            try
            {
                var blogs = await _blogDAO.GetAllAsync();
                return Ok(blogs);
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

        [HttpGet("GetById/{blogId}")]
        public async Task<ActionResult<BlogDTO>> GetById(string blogId)
        {
            try
            {
                var blog = await _blogDAO.GetByIdAsync(blogId);
                if (blog == null)
                    return NotFound($"Blog with ID {blogId} not found.");
                return Ok(blog);
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
        public async Task<ActionResult<BlogDTO>> Create([FromBody] Blog blog)
        {
            try
            {
                var createdBlog = await _blogDAO.CreateAsync(blog);
                return CreatedAtAction(nameof(GetById), new { blogId = createdBlog.BlogId }, createdBlog);
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
        public async Task<ActionResult<BlogDTO>> Update([FromBody] BlogDTO blogDto)
        {
            try
            {
                var blog = new Blog
                {
                    BlogId = blogDto.BlogId,
                    StaffId = blogDto.StaffId,
                    Title = blogDto.Title,
                    Content = blogDto.Content,
                    Status = blogDto.Status,
                    BlogTopicId = blogDto.BlogTopicId
                };
                var updatedBlog = await _blogDAO.UpdateAsync(blog);
                return Ok(updatedBlog);
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

        [HttpDelete("Delete/{blogId}")]
        public async Task<ActionResult<bool>> Delete(string blogId)
        {
            try
            {
                var result = await _blogDAO.DeleteAsync(blogId);
                if (!result)
                    return NotFound($"Blog with ID {blogId} not found.");
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
