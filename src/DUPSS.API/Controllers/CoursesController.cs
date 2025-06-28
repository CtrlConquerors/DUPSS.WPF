using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.AccessLayer.Interfaces; // Thêm namespace này để sử dụng ICourseDAO
using DUPSS.API.Models.DTOs; // Thêm namespace này để sử dụng CourseDTO
using DUPSS.API.Models.Objects; // Đảm bảo namespace này được bao gồm cho Course domain model
using Microsoft.AspNetCore.Mvc;
using Npgsql; // Thêm namespace này cho NpgsqlException

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseDAO _courseDAO; // Sử dụng giao diện, không phải kiểu cụ thể

        // Hàm tạo phải nhận ICourseDAO thông qua Dependency Injection
        public CoursesController(ICourseDAO courseDAO)
        {
            _courseDAO = courseDAO; // Gán thể hiện đã được inject
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetAll()
        {
            try
            {
                var courses = await _courseDAO.GetAllAsync();
                return Ok(courses);
            }
            catch (NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetById/{courseId}")]
        public async Task<ActionResult<CourseDTO>> GetById(string courseId)
        {
            try
            {
                var course = await _courseDAO.GetByIdAsync(courseId);
                if (course == null)
                    return NotFound($"Course with ID {courseId} not found.");
                return Ok(course);
            }
            catch (NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("Count")]
        public async Task<ActionResult<int>> GetCount()
        {
            try
            {
                // Replace _userDAO.CountAsync() with a manual count operation
                var courses = await _courseDAO.GetAllAsync();
                var count = courses.Count;
                return Ok(count);
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
        // Thay đổi tham số từ Course sang CourseDTO
        public async Task<ActionResult<CourseDTO>> Create([FromBody] CourseDTO courseDto)
        {
            try
            {
                // Ánh xạ CourseDTO sang Course domain model trước khi truyền cho DAO
                var courseDomainModel = new Course
                {
                    CourseId = courseDto.CourseId,
                    TopicId = courseDto.TopicId,
                    CourseName = courseDto.CourseName,
                    CourseType = courseDto.CourseType,
                    StaffId = courseDto.StaffId,
                    Description = courseDto.Description, // Bao gồm Description
                    ConsultantId = courseDto.ConsultantId // Bao gồm ConsultantId
                    // Các thuộc tính khác như ImageUrl, CreatedDate, Status, Inventory, IsSelected
                    // không cần ánh xạ ngược vì chúng là [NotMapped] hoặc được quản lý bởi backend.
                };

                var createdCourse = await _courseDAO.CreateAsync(courseDomainModel);
                return CreatedAtAction(nameof(GetById), new { courseId = createdCourse.CourseId }, createdCourse);
            }
            catch (NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("Update")]
        // Thay đổi tham số từ Course sang CourseDTO
        public async Task<ActionResult<CourseDTO>> Update([FromBody] CourseDTO courseDto)
        {
            try
            {
                // Ánh xạ CourseDTO sang Course domain model trước khi truyền cho DAO
                var courseDomainModel = new Course
                {
                    CourseId = courseDto.CourseId,
                    TopicId = courseDto.TopicId,
                    CourseName = courseDto.CourseName,
                    CourseType = courseDto.CourseType,
                    StaffId = courseDto.StaffId,
                    Description = courseDto.Description, // Bao gồm Description
                    ConsultantId = courseDto.ConsultantId // Bao gồm ConsultantId
                    // Đảm bảo tất cả các thuộc tính có thể cập nhật đều được ánh xạ
                };

                var updatedCourse = await _courseDAO.UpdateAsync(courseDomainModel);
                return Ok(updatedCourse);
            }
            catch (NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("Delete/{courseId}")]
        public async Task<ActionResult<bool>> Delete(string courseId)
        {
            try
            {
                var result = await _courseDAO.DeleteAsync(courseId);
                if (!result)
                    return NotFound($"Course with ID {courseId} not found.");
                return Ok(result);
            }
            catch (NpgsqlException ex)
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
