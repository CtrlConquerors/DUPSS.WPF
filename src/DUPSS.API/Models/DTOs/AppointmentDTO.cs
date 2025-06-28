namespace DUPSS.API.Models.DTOs
{
    public class AppointmentDTO
    {
        public required string AppointmentId { get; set; }
        public required string MemberId { get; set; }
        public required string ConsultantId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public required string Status { get; set; }
        public required string Topic { get; set; }
        public string? Notes { get; set; }
        public UserDTO? Member { get; set; }
        public UserDTO? Consultant { get; set; }
    }
}
