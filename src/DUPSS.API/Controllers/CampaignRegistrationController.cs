
using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.AspNetCore.Mvc;

namespace DUPSS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignRegistrationController : ControllerBase
    {
        private readonly ICampaignRegistrationDAO _dao;

        public CampaignRegistrationController(ICampaignRegistrationDAO dao)
        {
            _dao = dao;
        }

        // GET: api/CampaignRegistration
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CampaignRegistrationDTO>>> GetAll()
        {
            var registrations = await _dao.GetAllAsync();
            var result = registrations.Select(r => new CampaignRegistrationDTO
            {
                RegistrationId = r.RegistrationId,
                MemberId = r.MemberId,
                CampaignId = r.CampaignId,
                RegisteredAt = r.RegisteredAt
            });

            return Ok(result);
        }

        // GET: api/CampaignRegistration/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<CampaignRegistrationDTO>>> GetByUserId(string userId)
        {
            var registrations = await _dao.GetByUserIdAsync(userId);
            var result = registrations.Select(r => new CampaignRegistrationDTO
            {
                RegistrationId = r.RegistrationId,
                MemberId = r.MemberId,
                CampaignId = r.CampaignId,
                RegisteredAt = r.RegisteredAt
            });

            return Ok(result);
        }

        // GET: api/CampaignRegistration/campaign/{campaignId}
        [HttpGet("campaign/{campaignId}")]
        public async Task<ActionResult<IEnumerable<CampaignRegistrationDTO>>> GetByCampaignId(string campaignId)
        {
            var registrations = await _dao.GetByCampaignIdAsync(campaignId);
            var result = registrations.Select(r => new CampaignRegistrationDTO
            {
                RegistrationId = r.RegistrationId,
                MemberId = r.MemberId,
                CampaignId = r.CampaignId,
                RegisteredAt = r.RegisteredAt
            });

            return Ok(result);
        }

        // POST: api/CampaignRegistration
        [HttpPost]
        public async Task<ActionResult> Register([FromBody] CampaignRegistration registration)
        {

            if (await _dao.ExistsAsync(registration.MemberId, registration.CampaignId))
            {
                return Conflict("User already registered for this campaign.");
            }

            registration.RegistrationId = Guid.NewGuid().ToString();
            registration.RegisteredAt = DateTime.UtcNow;

            await _dao.AddAsync(registration);

            return CreatedAtAction(nameof(GetByUserId), new { userId = registration.MemberId }, registration);
        }

        // DELETE: api/CampaignRegistration/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var existing = await _dao.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            await _dao.DeleteAsync(id);
            return NoContent();
        }
        [HttpGet("user/{userId}/campaigns")]
        public async Task<ActionResult<IEnumerable<CampaignDTO>>> GetCampaignsByUserId(string userId)
        {
            try
            {
                var registrations = await _dao.GetByUserIdWithCampaignAsync(userId);
                if (registrations == null || !registrations.Any())
                {
                    return NotFound($"No campaign registrations found for user ID: {userId}");
                }

                var result = registrations
                    .Where(r => r.Campaign != null)
                    .Select(r => new CampaignDTO
                    {
                        CampaignId = r.Campaign.CampaignId,
                        Title = r.Campaign.Title,
                        Description = r.Campaign.Description,
                        StartDate = r.Campaign.StartDate,
                        EndDate = r.Campaign.EndDate,
                        Status = r.Campaign.Status,
                        StaffId = r.Campaign.StaffId,
                        Location = r.Campaign.Location,
                        Introduction = r.Campaign.Introduction,
                        Staff = r.Campaign.Staff != null ? new UserDTO
                        {
                            UserId = r.Campaign.Staff.UserId,
                            Username = r.Campaign.Staff.Username,
                            Email = r.Campaign.Staff.Email,
                            PhoneNumber = r.Campaign.Staff.PhoneNumber,
                            DoB = r.Campaign.Staff.DoB,
                            RoleId = r.Campaign.Staff.RoleId,
                            Role = r.Campaign.Staff.Role != null ? new RoleDTO
                            {
                                RoleId = r.Campaign.Staff.Role.RoleId,
                                RoleName = r.Campaign.Staff.Role.RoleName
                            } : null
                        } : null
                    }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // DELETE: api/CampaignRegistration?memberId=xxx&campaignId=yyy
        [HttpDelete]
        public async Task<IActionResult> Unregister([FromQuery] string memberId, [FromQuery] string campaignId)
        {
            var existing = await _dao.GetByMemberAndCampaignAsync(memberId, campaignId);
            if (existing == null)
                return NotFound("Registration not found.");

            await _dao.DeleteAsync(existing.RegistrationId);
            return NoContent();
        }



    }
}
