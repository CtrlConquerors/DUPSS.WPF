using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;

namespace DUPSS.API.Models.AccessLayer.DAOs
{
    public class CampaignDAO : ICampaignDAO
    {
        private readonly AppDbContext _context;

        public CampaignDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CampaignDTO> CreateAsync(Campaign campaign)
        {
            _context.Campaign.Add(campaign);
            await _context.SaveChangesAsync();
            return new CampaignDTO
            {
                CampaignId = campaign.CampaignId,
                StaffId = campaign.StaffId,
                Title = campaign.Title,
                ImageUrl = $"images/{campaign.CampaignId}.jpg",
                Description = campaign.Description,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Status = campaign.Status,
                Location = campaign.Location,
                Introduction = campaign.Introduction,
                Duration = campaign.EndDate.HasValue ? (TimeSpan?)(campaign.EndDate.Value.ToDateTime(new TimeOnly(0)) - campaign.StartDate.ToDateTime(new TimeOnly(0))) : null
            };
        }

        public async Task<CampaignDTO?> GetByIdAsync(string campaignId)
        {
            return await _context.Campaign
                .Where(c => c.CampaignId == campaignId)
                .Select(c => new CampaignDTO
                {
                    CampaignId = c.CampaignId,
                    StaffId = c.StaffId,
                    Title = c.Title,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Location = c.Location,
                    Introduction = c.Introduction,
                    Status = c.Status,
                    Duration = c.EndDate.HasValue ? (TimeSpan?)(c.EndDate.Value.ToDateTime(new TimeOnly(0)) - c.StartDate.ToDateTime(new TimeOnly(0))) : null,
                    Staff = c.Staff != null ? new UserDTO
                    {
                        UserId = c.Staff.UserId,
                        Username = c.Staff.Username,
                        DoB = c.Staff.DoB,
                        PhoneNumber = c.Staff.PhoneNumber,
                        Email = c.Staff.Email,
                        RoleId = c.Staff.RoleId

                    } : null,
                    ImageUrl = $"/images/{c.CampaignId}.jpg"
                })

                .FirstOrDefaultAsync();
        }

        public async Task<List<CampaignDTO>> GetAllAsync()
        {
            return await _context.Campaign
                .Select(c => new CampaignDTO
                {
                    CampaignId = c.CampaignId,
                    StaffId = c.StaffId,
                    Title = c.Title,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status,
                    Location = c.Location,
                    Introduction = c.Introduction,      
                    Duration = c.EndDate.HasValue ? (TimeSpan?)(c.EndDate.Value.ToDateTime(new TimeOnly(0)) - c.StartDate.ToDateTime(new TimeOnly(0))) : null,
                    Staff = c.Staff != null ? new UserDTO
                    {
                        UserId = c.Staff.UserId,
                        Username = c.Staff.Username,
                        DoB = c.Staff.DoB,
                        PhoneNumber = c.Staff.PhoneNumber,
                        Email = c.Staff.Email,
                        RoleId = c.Staff.RoleId
                    } : null,
                    ImageUrl = $"/images/{c.CampaignId}.jpg"
                })
                .ToListAsync();
        }

        public async Task<CampaignDTO> UpdateAsync(CampaignDTO campaign)
        {
            var existingCampaign = await _context.Campaign.FindAsync(campaign.CampaignId);
            if (existingCampaign == null)
                throw new Exception($"Campaign with ID {campaign.CampaignId} not found.");

            existingCampaign.StaffId = campaign.StaffId;
            existingCampaign.Title = campaign.Title;
            existingCampaign.Description = campaign.Description;
            existingCampaign.StartDate = campaign.StartDate;
            existingCampaign.EndDate = campaign.EndDate;
            existingCampaign.Location = campaign.Location;
            existingCampaign.Introduction = campaign.Introduction;
            existingCampaign.Status = campaign.Status;

            await _context.SaveChangesAsync();
            return new CampaignDTO
            {
                CampaignId = existingCampaign.CampaignId,
                StaffId = existingCampaign.StaffId,
                Title = existingCampaign.Title,
                Description = existingCampaign.Description,
                StartDate = existingCampaign.StartDate,
                EndDate = existingCampaign.EndDate,
                Status = existingCampaign.Status,
                Duration = existingCampaign.EndDate.HasValue ? (TimeSpan?)(existingCampaign.EndDate.Value.ToDateTime(new TimeOnly(0)) - existingCampaign.StartDate.ToDateTime(new TimeOnly(0))) : null,
                Location = existingCampaign.Location,
                Introduction = existingCampaign.Introduction
            };
        }

        public async Task<bool> DeleteAsync(string campaignId)
        {
            var campaign = await _context.Campaign.FindAsync(campaignId);
            if (campaign == null)
                return false;

            _context.Campaign.Remove(campaign);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Campaign.CountAsync();
        }
    }
}
