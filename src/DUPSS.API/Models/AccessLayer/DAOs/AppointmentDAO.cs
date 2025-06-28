using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace DUPSS.API.Models.AccessLayer.DAOs
{
    public class AppointmentDAO : IAppointmentDAO
    {
        private readonly AppDbContext _context;

        public AppointmentDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AppointmentDTO> CreateAsync(Appointment appointment)
        {
            var lastAppointment = await _context.Appointments
                .OrderByDescending(a => a.AppointmentId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastAppointment != null && lastAppointment.AppointmentId.StartsWith("AP"))
            {
                var lastId = lastAppointment.AppointmentId;
                if (int.TryParse(lastId[2..], out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            appointment.AppointmentId = $"AP{nextNumber:D4}";

            // ✅ Chuyển sang UTC trước khi lưu
            appointment.AppointmentDate = appointment.AppointmentDate.ToUniversalTime();

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return new AppointmentDTO
            {
                AppointmentId = appointment.AppointmentId,
                MemberId = appointment.MemberId,
                ConsultantId = appointment.ConsultantId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                Topic = appointment.Topic,
                Notes = appointment.Notes
            };
        }

        public async Task<int> CountAsync()
        {
            return await _context.Appointment.CountAsync();
        }


        public async Task<AppointmentDTO?> GetByIdAsync(string appointmentId)
        {
            return await _context.Appointment
                .Where(a => a.AppointmentId == appointmentId)
                .Select(a => new AppointmentDTO
                {
                    AppointmentId = a.AppointmentId,
                    MemberId = a.MemberId,
                    ConsultantId = a.ConsultantId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Topic = a.Topic,
                    Notes = a.Notes,
                    Member = a.Member != null ? new UserDTO
                    {
                        UserId = a.Member.UserId,
                        Username = a.Member.Username,
                        DoB = a.Member.DoB,
                        PhoneNumber = a.Member.PhoneNumber,
                        Email = a.Member.Email,
                        RoleId = a.Member.RoleId
                    } : null,
                    Consultant = a.Consultant != null ? new UserDTO
                    {
                        UserId = a.Consultant.UserId,
                        Username = a.Consultant.Username,
                        DoB = a.Consultant.DoB,
                        PhoneNumber = a.Consultant.PhoneNumber,
                        Email = a.Consultant.Email,
                        RoleId = a.Consultant.RoleId
                    } : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<AppointmentDTO>> GetAllAsync()
        {
            return await _context.Appointment
                .Select(a => new AppointmentDTO
                {
                    AppointmentId = a.AppointmentId,
                    MemberId = a.MemberId,
                    ConsultantId = a.ConsultantId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Topic = a.Topic,
                    Notes = a.Notes,
                    Member = a.Member != null ? new UserDTO
                    {
                        UserId = a.Member.UserId,
                        Username = a.Member.Username,
                        DoB = a.Member.DoB,
                        PhoneNumber = a.Member.PhoneNumber,
                        Email = a.Member.Email,
                        RoleId = a.Member.RoleId
                    } : null,
                    Consultant = a.Consultant != null ? new UserDTO
                    {
                        UserId = a.Consultant.UserId,
                        Username = a.Consultant.Username,
                        DoB = a.Consultant.DoB,
                        PhoneNumber = a.Consultant.PhoneNumber,
                        Email = a.Consultant.Email,
                        RoleId = a.Consultant.RoleId
                    } : null
                })
                .ToListAsync();
        }

        public async Task<AppointmentDTO> UpdateAsync(Appointment appointment)
        {
            var existingAppointment = await _context.Appointment.FindAsync(appointment.AppointmentId);
            if (existingAppointment == null)
                throw new Exception($"Appointment with ID {appointment.AppointmentId} not found.");

            existingAppointment.MemberId = appointment.MemberId;
            existingAppointment.ConsultantId = appointment.ConsultantId;
            existingAppointment.AppointmentDate = appointment.AppointmentDate;
            existingAppointment.Status = appointment.Status;
            existingAppointment.Topic = appointment.Topic;
            existingAppointment.Notes = appointment.Notes;

            await _context.SaveChangesAsync();
            return new AppointmentDTO
            {
                AppointmentId = existingAppointment.AppointmentId,
                MemberId = existingAppointment.MemberId,
                ConsultantId = existingAppointment.ConsultantId,
                AppointmentDate = existingAppointment.AppointmentDate,
                Status = existingAppointment.Status,
                Topic = existingAppointment.Topic,
                Notes = existingAppointment.Notes
            };
        }

        public async Task<bool> DeleteAsync(string appointmentId)
        {
            var appointment = await _context.Appointment.FindAsync(appointmentId);
            if (appointment == null)
                return false;

            _context.Appointment.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

    
        public async Task<List<AppointmentDTO>> GetByMemberIdAsync(string memberId)
        {
            var appointments = await _context.Appointment
                .Include(a => a.Consultant)
                .Include(a => a.Member)
                .Where(a => a.MemberId == memberId)
                .ToListAsync();

            return appointments.Select(a => new AppointmentDTO
            {
                AppointmentId = a.AppointmentId,
                MemberId = a.MemberId,
                ConsultantId = a.ConsultantId,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status,
                Topic = a.Topic,
                Notes = a.Notes,
                Member = a.Member != null ? new UserDTO
                {
                    UserId = a.Member.UserId,
                    Username = a.Member.Username,
                    DoB = a.Member.DoB,
                    PhoneNumber = a.Member.PhoneNumber,
                    Email = a.Member.Email,
                    ImageUrl = $"images/{a.Member.UserId}.jpg",
                    RoleId = a.Member.RoleId,
                    Role = a.Member.Role != null ? new RoleDTO
                    {
                        RoleId = a.Member.Role.RoleId,
                        RoleName = a.Member.Role.RoleName
                    } : null
                } : null,
                Consultant = a.Consultant != null ? new UserDTO
                {
                    UserId = a.Consultant.UserId,
                    Username = a.Consultant.Username,
                    DoB = a.Consultant.DoB,
                    PhoneNumber = a.Consultant.PhoneNumber,
                    Email = a.Consultant.Email,
                    ImageUrl = $"images/{a.Consultant.UserId}.jpg",
                    RoleId = a.Consultant.RoleId,
                    Role = a.Consultant.Role != null ? new RoleDTO
                    {
                        RoleId = a.Consultant.Role.RoleId,
                        RoleName = a.Consultant.Role.RoleName
                    } : null
                } : null
            }).ToList();
        }

        public async Task<List<AppointmentDTO>> GetByConsultantIdAsync(string consultantId)
        {
            var appointments = await _context.Appointment
                .Include(a => a.Consultant)
                .Include(a => a.Member)
                .Where(a => a.ConsultantId == consultantId)
                .ToListAsync();

            return appointments.Select(a => new AppointmentDTO
            {
                AppointmentId = a.AppointmentId,
                MemberId = a.MemberId,
                ConsultantId = a.ConsultantId,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status,
                Topic = a.Topic,
                Notes = a.Notes,

                Member = a.Member != null ? new UserDTO
                {
                    UserId = a.Member.UserId,
                    Username = a.Member.Username,
                    DoB = a.Member.DoB,
                    PhoneNumber = a.Member.PhoneNumber,
                    Email = a.Member.Email,
                    ImageUrl = $"images/{a.Member.UserId}.jpg",
                    RoleId = a.Member.RoleId,
                    Role = a.Member.Role != null ? new RoleDTO
                    {
                        RoleId = a.Member.Role.RoleId,
                        RoleName = a.Member.Role.RoleName
                    } : null
                } : null,

                Consultant = a.Consultant != null ? new UserDTO
                {
                    UserId = a.Consultant.UserId,
                    Username = a.Consultant.Username,
                    DoB = a.Consultant.DoB,
                    PhoneNumber = a.Consultant.PhoneNumber,
                    Email = a.Consultant.Email,
                    ImageUrl = $"images/{a.Consultant.UserId}.jpg",
                    RoleId = a.Consultant.RoleId,
                    Role = a.Consultant.Role != null ? new RoleDTO
                    {
                        RoleId = a.Consultant.Role.RoleId,
                        RoleName = a.Consultant.Role.RoleName
                    } : null
                } : null

            }).ToList();
        }

        public async Task<bool> UpdateStatusAsync(string appointmentId, string newStatus)
        {
            // Tìm lịch hẹn theo ID
            var appointment = await _context.Appointment.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                return false; // Không tìm thấy
            }

            // Cập nhật trạng thái mới
            appointment.Status = newStatus;

            // Lưu thay đổi
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> RescheduleAsync(string appointmentId, DateTime newDateUtc)
        {
            var appt = await _context.Appointment.FindAsync(appointmentId);
            if (appt == null) return false;

            appt.AppointmentDate = newDateUtc;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateNotesAsync(string appointmentId, string notes)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return false;

            appointment.Notes = notes;
            await _context.SaveChangesAsync();
            return true;
        }




    }
}
