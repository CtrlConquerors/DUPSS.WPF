using DUPSS.DAO.Interfaces;
using DUPSS.DTO.DTOs;
using DUPSS.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; // Sử dụng SqlException thay vì NpgsqlException
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DUPSS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseDAO _courseDAO;

        public CoursesController(ICourseDAO courseDAO)
        {
            _courseDAO = courseDAO;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetAll()
        {
            try
            {
                var courses = await _courseDAO.GetAllAsync();
                return Ok(courses);
            }
            catch (SqlException ex)
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
            catch (SqlException ex)
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
                var courses = await _courseDAO.GetAllAsync();
                return Ok(courses.Count);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult<CourseDTO>> Create([FromBody] CourseDTO courseDto)
        {
            try
            {
                var course = new Course
                {
                    CourseId = courseDto.CourseId,
                    TopicId = courseDto.TopicId,
                    CourseName = courseDto.CourseName,
                    CourseType = courseDto.CourseType,
                    StaffId = courseDto.StaffId,
                    Description = courseDto.Description,
                    ConsultantId = courseDto.ConsultantId
                    // Các trường backend tự sinh (CreatedDate, Status, ...) không cần set
                };

                var createdCourse = await _courseDAO.CreateAsync(course);
                return CreatedAtAction(nameof(GetById), new { courseId = createdCourse.CourseId }, createdCourse);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("Update")]
        public async Task<ActionResult<CourseDTO>> Update([FromBody] CourseDTO courseDto)
        {
            try
            {
                var course = new Course
                {
                    CourseId = courseDto.CourseId,
                    TopicId = courseDto.TopicId,
                    CourseName = courseDto.CourseName,
                    CourseType = courseDto.CourseType,
                    StaffId = courseDto.StaffId,
                    Description = courseDto.Description,
                    ConsultantId = courseDto.ConsultantId
                };

                var updated = await _courseDAO.UpdateAsync(course);
                return Ok(updated);
            }
            catch (SqlException ex)
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
            catch (SqlException ex)
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
