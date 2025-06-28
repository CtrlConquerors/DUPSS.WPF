using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;


namespace DUPSS.API.Models.AccessLayer.DAOs
{
    public class CampaignRegistrationDAO : ICampaignRegistrationDAO
    {
        private readonly AppDbContext _context;

        public CampaignRegistrationDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CampaignRegistration>> GetAllAsync()
        {
            return await _context.CampaignRegistrations
                                 .Include(r => r.User)
                                 .Include(r => r.Campaign)
                                 .ToListAsync();
        }

        public async Task<CampaignRegistration?> GetByIdAsync(string registrationId)
        {
            return await _context.CampaignRegistrations
                                 .Include(r => r.User)
                                 .Include(r => r.Campaign)
                                 .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);
        }

        public async Task<IEnumerable<CampaignRegistration>> GetByUserIdAsync(string userId)
        {
            return await _context.CampaignRegistrations
                                 .Include(r => r.Campaign)
                                 .Where(r => r.MemberId == userId)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<CampaignRegistration>> GetByCampaignIdAsync(string campaignId)
        {
            return await _context.CampaignRegistrations
                                 .Include(r => r.User)
                                 .Where(r => r.CampaignId == campaignId)
                                 .ToListAsync();
        }

        public async Task AddAsync(CampaignRegistration registration)
        {
            await _context.CampaignRegistrations.AddAsync(registration);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string registrationId)
        {
            var existing = await _context.CampaignRegistrations.FindAsync(registrationId);
            if (existing != null)
            {
                _context.CampaignRegistrations.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string userId, string campaignId)
        {
            return await _context.CampaignRegistrations
                                 .AnyAsync(r => r.MemberId == userId && r.CampaignId == campaignId);
        }
        public async Task<List<CampaignRegistration>> GetByUserIdWithCampaignAsync(string userId)
        {
            return await _context.CampaignRegistrations
                .Include(r => r.Campaign)
                .Where(r => r.MemberId == userId)
                .ToListAsync();
        }
        public async Task<CampaignRegistration?> GetByMemberAndCampaignAsync(string memberId, string campaignId)
        {
            return await _context.CampaignRegistrations
                .FirstOrDefaultAsync(r => r.MemberId == memberId && r.CampaignId == campaignId);
        }



    }
}
