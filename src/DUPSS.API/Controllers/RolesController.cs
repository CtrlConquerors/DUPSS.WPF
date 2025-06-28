using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.AspNetCore.Mvc;

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleDAO _roleDAO;

        public RolesController(AppDbContext context)
        {
            _roleDAO = new RoleDAO(context);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetAll()
        {
            try
            {
                var roles = await _roleDAO.GetAllAsync();
                return Ok(roles);
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

        [HttpGet("GetById/{roleId}")]
        public async Task<ActionResult<RoleDTO>> GetById(string roleId)
        {
            try
            {
                var role = await _roleDAO.GetByIdAsync(roleId);
                if (role == null)
                    return NotFound($"Role with ID {roleId} not found.");
                return Ok(role);
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
        public async Task<ActionResult<RoleDTO>> Create([FromBody] Role role)
        {
            try
            {
                var createdRole = await _roleDAO.CreateAsync(role);
                return CreatedAtAction(nameof(GetById), new { roleId = createdRole.RoleId }, createdRole);
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
        public async Task<ActionResult<RoleDTO>> Update([FromBody] Role role)
        {
            try
            {
                var updatedRole = await _roleDAO.UpdateAsync(role);
                return Ok(updatedRole);
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

        [HttpDelete("Delete/{roleId}")]
        public async Task<ActionResult<bool>> Delete(string roleId)
        {
            try
            {
                var result = await _roleDAO.DeleteAsync(roleId);
                if (!result)
                    return NotFound($"Role with ID {roleId} not found.");
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
