using DUPSS.DTO.DTOs;
// using DUPSS.API.Models.Objects; // Không cần thiết nếu bạn chỉ dùng UserDTO ở đây

namespace DUPSS.Common
{
    public class CreateUserRequest
    {
        // Thay đổi kiểu của thuộc tính User từ 'User' thành 'UserDTO'
        public required UserDTO User { get; set; }
        public required string Password { get; set; }
    }
}
