using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;

namespace DUPSS.API.Models.AccessLayer.Interfaces
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
