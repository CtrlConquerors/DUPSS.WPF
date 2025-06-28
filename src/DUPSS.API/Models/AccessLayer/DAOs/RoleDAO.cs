using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;

namespace DUPSS.API.Models.AccessLayer.DAOs
{
    public class RoleDAO : IRoleDAO
    {
        private readonly AppDbContext _context;

        public RoleDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RoleDTO> CreateAsync(Role role)
        {
            _context.Role.Add(role);
            await _context.SaveChangesAsync();
            return new RoleDTO
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            };
        }

        public async Task<RoleDTO?> GetByIdAsync(string roleId)
        {
            return await _context.Role
                .Where(r => r.RoleId == roleId)
                .Select(r => new RoleDTO
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<RoleDTO>> GetAllAsync()
        {
            return await _context.Role
                .Select(r => new RoleDTO
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToListAsync();
        }

        public async Task<RoleDTO> UpdateAsync(Role role)
        {
            var existingRole = await _context.Role.FindAsync(role.RoleId);
            if (existingRole == null)
                throw new Exception($"Role with ID {role.RoleId} not found.");

            existingRole.RoleName = role.RoleName;

            await _context.SaveChangesAsync();
            return new RoleDTO
            {
                RoleId = existingRole.RoleId,
                RoleName = existingRole.RoleName
            };
        }

        public async Task<bool> DeleteAsync(string roleId)
        {
            var role = await _context.Role.FindAsync(roleId);
            if (role == null)
                return false;

            _context.Role.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
