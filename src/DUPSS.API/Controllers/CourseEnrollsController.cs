using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects; // Ensure this namespace is included for CourseEnroll domain model
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CourseEnrollsController : ControllerBase
    {
        private readonly ICourseEnrollDAO _courseEnrollDAO;

        public CourseEnrollsController(ICourseEnrollDAO courseEnrollDAO)
        {
            _courseEnrollDAO = courseEnrollDAO;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<CourseEnrollDTO>>> GetAll()
        {
            try
            {
                var enrolls = await _courseEnrollDAO.GetAllAsync();
                return Ok(enrolls);
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error in GetAll: {ex.Message}");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error in GetAll: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetById/{enrollId}")]
        public async Task<ActionResult<CourseEnrollDTO>> GetById(string enrollId)
        {
            try
            {
                var enroll = await _courseEnrollDAO.GetByIdAsync(enrollId);
                if (enroll == null)
                    return NotFound($"CourseEnroll with ID {enrollId} not found.");
                return Ok(enroll);
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error in GetById({enrollId}): {ex.Message}");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error in GetById({enrollId}): {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("Count")]
        public async Task<ActionResult<int>> GetCount()
        {
            try
            {
                // Replace _userDAO.CountAsync() with a manual count operation
                var courses = await _courseEnrollDAO.GetAllAsync();
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
        public async Task<ActionResult<CourseEnrollDTO>> Create([FromBody] CourseEnrollDTO courseEnrollDto)
        {
            try
            {
                // Map CourseEnrollDTO to CourseEnroll domain model
                var courseEnrollDomainModel = new CourseEnroll
                {
                    EnrollId = courseEnrollDto.EnrollId, // Client-generated ID is accepted
                    MemberId = courseEnrollDto.MemberId,
                    CourseId = courseEnrollDto.CourseId,
                    Status = courseEnrollDto.Status,
                    EnrollDate = courseEnrollDto.EnrollDate,
                    CompleteDate = courseEnrollDto.CompleteDate
                    // Navigation properties (Member, Course) are not mapped here,
                    // as the DAO primarily uses their IDs.
                };

                var createdEnroll = await _courseEnrollDAO.CreateAsync(courseEnrollDomainModel);
                return CreatedAtAction(nameof(GetById), new { enrollId = createdEnroll.EnrollId }, createdEnroll);
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error in Create: {ex.Message}");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error in Create: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("Update")]
        public async Task<ActionResult<CourseEnrollDTO>> Update([FromBody] CourseEnrollDTO courseEnrollDto)
        {
            try
            {
                // Map CourseEnrollDTO to CourseEnroll domain model
                // Note: For update, you usually fetch the existing entity first,
                // then update its scalar properties from the DTO.
                var courseEnrollDomainModel = new CourseEnroll
                {
                    EnrollId = courseEnrollDto.EnrollId,
                    MemberId = courseEnrollDto.MemberId,
                    CourseId = courseEnrollDto.CourseId,
                    Status = courseEnrollDto.Status,
                    EnrollDate = courseEnrollDto.EnrollDate,
                    CompleteDate = courseEnrollDto.CompleteDate
                };

                var updatedEnroll = await _courseEnrollDAO.UpdateAsync(courseEnrollDomainModel);
                return Ok(updatedEnroll);
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error in Update: {ex.Message}");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error in Update: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("Delete/{enrollId}")]
        public async Task<ActionResult<bool>> Delete(string enrollId)
        {
            try
            {
                var result = await _courseEnrollDAO.DeleteAsync(enrollId);
                if (!result)
                    return NotFound($"CourseEnroll with ID {enrollId} not found for deletion.");
                return Ok(result);
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error in Delete({enrollId}): {ex.Message}");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error in Delete({enrollId}): {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetByMemberAndCourse")]
        public async Task<ActionResult<List<CourseEnrollDTO>>> GetByMemberAndCourse([FromQuery] string memberId, [FromQuery] string courseId)
        {
            try
            {
                var enrollments = await _courseEnrollDAO.GetEnrollmentsByMemberAndCourse(memberId, courseId);
                return Ok(enrollments);
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error in GetByMemberAndCourse({memberId}, {courseId}): {ex.Message}");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error in GetByMemberAndCourse({memberId}, {courseId}): {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
