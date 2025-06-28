// Fix 1: Ensure consistent API endpoint usage in `AppointmentApiService`
// Fix 2: Align DTO vs domain model expectations
// Fix 3: Rename POST endpoint to accept DTO instead of domain model

using DUPSS.API.Models.DTOs;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace DUPSS.Web.Components.Service
{
    public class AppointmentApiService : GenericApiService<AppointmentDTO>
    {
        private readonly HttpClient _httpClient;

        public AppointmentApiService(HttpClient httpClient)
            : base(httpClient, "api/Appointments")
        {
            _httpClient = httpClient;
        }

        public async Task<List<AppointmentDTO>> GetAppointmentsForMemberAsync(string memberId)
        {
            var response = await _httpClient.GetAsync($"api/Appointments/by-member/{memberId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<AppointmentDTO>>() ?? new List<AppointmentDTO>();
            }
            return new List<AppointmentDTO>();
        }

        public async Task<bool> CreateAppointmentAsync(AppointmentDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Appointments", dto);
            return response.IsSuccessStatusCode;
        }
        public async Task<bool> CancelAppointmentAsync(string appointmentId)
        {
            var response = await _httpClient.DeleteAsync($"api/Appointments/Delete/{appointmentId}");
            return response.IsSuccessStatusCode;
        }


        public async Task<List<AppointmentDTO>> GetAppointmentsForConsultantAsync(string consultantId)
        {
            var response = await _httpClient.GetAsync($"api/Appointments/by-consultant/{consultantId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<AppointmentDTO>>() ?? new();
            }

           
            return new List<AppointmentDTO>();
        }

        public async Task<bool> UpdateAppointmentStatusAsync(string appointmentId, string status)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Appointments/{appointmentId}/status", status);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RescheduleAppointmentAsync(string appointmentId, DateTime newDateUtc)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/Appointments/{appointmentId}/reschedule",
                new { AppointmentDate = newDateUtc });

            return response.IsSuccessStatusCode;
        }
        public async Task<bool> UpdateAppointmentNotesAsync(string appointmentId, string notes)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Appointments/{appointmentId}/notes", notes);
            return response.IsSuccessStatusCode;
        }

        public async Task<int> GetCountAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<int>("api/Appointments/Count");
            return result;
        }


    }
}
