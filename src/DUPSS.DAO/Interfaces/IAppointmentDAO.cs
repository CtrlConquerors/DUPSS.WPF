using DUPSS.DTO.DTOs;
using DUPSS.Objects;

namespace DUPSS.DAO.Interfaces
{
    public interface IAppointmentDAO
    {
        Task<AppointmentDTO> CreateAsync(Appointment appointment);
        Task<AppointmentDTO> GetByIdAsync(string appointmentId);
        Task<List<AppointmentDTO>> GetAllAsync();
        Task<AppointmentDTO> UpdateAsync(Appointment appointment);
        Task<bool> DeleteAsync(string appointmentId);
    }
}
