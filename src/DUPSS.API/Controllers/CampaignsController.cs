using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;

namespace DUPSS.API.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CampaignsController : ControllerBase
    {
        private readonly CampaignDAO _campaignDAO;

        public CampaignsController(AppDbContext context)
        {
            _campaignDAO = new CampaignDAO(context);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<CampaignDTO>>> GetAll()
        {
            try
            {
                var campaigns = await _campaignDAO.GetAllAsync();
                return Ok(campaigns);
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

        [HttpGet("GetById/{campaignId}")]
        public async Task<ActionResult<CampaignDTO>> GetById(string campaignId)
        {
            try
            {
                var campaign = await _campaignDAO.GetByIdAsync(campaignId);
                if (campaign == null)
                    return NotFound($"Campaign with ID {campaignId} not found.");
                return Ok(campaign);
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
            var count = await _campaignDAO.CountAsync();
            return Ok(count);
        }


        [HttpPost("Create")]
        public async Task<ActionResult<CampaignDTO>> Create([FromBody] Campaign campaign)
        {
            try
            {
                var createdCampaign = await _campaignDAO.CreateAsync(campaign);
                return CreatedAtAction(nameof(GetById), new { campaignId = createdCampaign.CampaignId }, createdCampaign);
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
        public async Task<ActionResult<CampaignDTO>> Update([FromBody] CampaignDTO campaign)
        {
            try
            {
                var updatedCampaign = await _campaignDAO.UpdateAsync(campaign);
                return Ok(updatedCampaign);
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

        [HttpDelete("Delete/{campaignId}")]
        public async Task<ActionResult<bool>> Delete(string campaignId)
        {
            try
            {
                var result = await _campaignDAO.DeleteAsync(campaignId);
                if (!result)
                    return NotFound($"Campaign with ID {campaignId} not found.");
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
