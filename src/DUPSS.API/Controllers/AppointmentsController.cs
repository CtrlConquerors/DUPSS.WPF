using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.AspNetCore.Mvc;

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentDAO _appointmentDAO;

        public AppointmentsController(AppDbContext context)
        {
            _appointmentDAO = new AppointmentDAO(context);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetAll()
        {
            try
            {
                var appointments = await _appointmentDAO.GetAllAsync();
                return Ok(appointments);
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
        [HttpGet("Count")]
        public async Task<ActionResult<int>> GetCount()
        {
            var count = await _appointmentDAO.CountAsync();
            return Ok(count);
        }
        [HttpGet("GetById/{appointmentId}")]
        public async Task<ActionResult<AppointmentDTO>> GetById(string appointmentId)
        {
            try
            {
                var appointment = await _appointmentDAO.GetByIdAsync(appointmentId);
                if (appointment == null)
                    return NotFound($"Appointment with ID {appointmentId} not found.");
                return Ok(appointment);
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
        public async Task<ActionResult<AppointmentDTO>> Create([FromBody] AppointmentDTO dto)
        {
            try
            {
               
                var appointment = new Appointment
                {
                    AppointmentId = "",
                    MemberId = dto.MemberId,
                    ConsultantId = dto.ConsultantId,
                    AppointmentDate = dto.AppointmentDate,
                    Status = dto.Status,
                    Topic = dto.Topic,
                    Notes = dto.Notes
                };

                var createdAppointment = await _appointmentDAO.CreateAsync(appointment);


                return CreatedAtAction(nameof(GetById), new { appointmentId = createdAppointment.AppointmentId }, createdAppointment);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }



        [HttpPut("Update")]
        public async Task<ActionResult<AppointmentDTO>> Update([FromBody] Appointment appointment)
        {
            try
            {
                var updatedAppointment = await _appointmentDAO.UpdateAsync(appointment);
                return Ok(updatedAppointment);
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

        [HttpDelete("Delete/{appointmentId}")]
        public async Task<ActionResult<bool>> Delete(string appointmentId)
        {
            try
            {
                var result = await _appointmentDAO.DeleteAsync(appointmentId);
                if (!result)
                    return NotFound($"Appointment with ID {appointmentId} not found.");
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
        [HttpGet("by-member/{memberId}")]
        public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetByMemberId(string memberId)
        {
            try
            {
                var appointments = await _appointmentDAO.GetByMemberIdAsync(memberId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("by-consultant/{consultantId}")]
        public async Task<ActionResult<List<AppointmentDTO>>> GetByConsultant(string consultantId)
        {
            var result = await _appointmentDAO.GetByConsultantIdAsync(consultantId);
            return Ok(result);
        }
        [HttpPost]
        public async Task<ActionResult<AppointmentDTO>> CreateAppointment([FromBody] AppointmentDTO dto)
        {
            if (dto == null)
                return BadRequest("Appointment data is missing.");


            if (string.IsNullOrWhiteSpace(dto.MemberId) ||
                string.IsNullOrWhiteSpace(dto.ConsultantId) ||
                string.IsNullOrWhiteSpace(dto.Topic))
            {
                return BadRequest("Missing required fields: MemberId, ConsultantId, or Topic.");
            }

            var appointment = new Appointment
            {
                AppointmentId = "",
                MemberId = dto.MemberId,
                ConsultantId = dto.ConsultantId,
                AppointmentDate = dto.AppointmentDate,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
                Topic = dto.Topic,
                Notes = dto.Notes
            };

            var createdDto = await _appointmentDAO.CreateAsync(appointment);
            return Ok(createdDto);
        }

        [HttpPut("{AppointmentId}/status")]
        public async Task<IActionResult> UpdateStatus(string appointmentId, [FromBody] string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return BadRequest("Status is required.");
            var success = await _appointmentDAO.UpdateStatusAsync(appointmentId, status);
            if (!success) return NotFound("Appointment not found or update failed.");
            return Ok();
        }

        [HttpPut("{id}/reschedule")]
        public async Task<IActionResult> RescheduleAppointment(string id, [FromBody] RescheduleDTO dto)
        {
            if (dto == null || dto.AppointmentDate == default)
                return BadRequest("Invalid date");

            var success = await _appointmentDAO.RescheduleAsync(id, dto.AppointmentDate);
            if (!success) return NotFound("Appointment not found");

            return NoContent();
        }

        public class RescheduleDTO
        {
            public DateTime AppointmentDate { get; set; }
        }
        [HttpPut("{AppointmentId}/notes")]
        public async Task<IActionResult> UpdateNotes(string appointmentId, [FromBody] string notes)
        {
            if (string.IsNullOrWhiteSpace(appointmentId))
                return BadRequest("Appointment ID is required.");

            var success = await _appointmentDAO.UpdateNotesAsync(appointmentId, notes);
            if (!success)
                return NotFound("Appointment not found or update failed.");

            return Ok();
        }



    }
}
