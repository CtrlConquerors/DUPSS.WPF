using DUPSS.DAO.Interfaces;
using DUPSS.DTO.DTOs;
using DUPSS.Objects;
using Microsoft.EntityFrameworkCore;
using DUPSS.DB;
namespace DUPSS.DAO.DAOs
{
    public class CourseDAO : ICourseDAO
    {
        private readonly AppDbContext _context;

        public CourseDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CourseDTO> CreateAsync(Course course)
        {
            _context.Course.Add(course);
            await _context.SaveChangesAsync();
            return new CourseDTO
            {
                CourseId = course.CourseId,
                TopicId = course.TopicId,
                CourseName = course.CourseName,
                CourseType = course.CourseType,
                StaffId = course.StaffId,
                Description = course.Description, // Include Description
                ConsultantId = course.ConsultantId, // Include ConsultantId
                // ImageUrl and ImageUrl2 are no longer generated here; WPF client will handle local paths
                CreatedDate = course.CreatedDate,
                Status = course.Status,
                Inventory = course.Inventory,
                IsSelected = course.IsSelected
            };
        }

        public async Task<CourseDTO?> GetByIdAsync(string courseId)
        {
            return await _context.Course
                .Include(c => c.Topic) // Eagerly load Topic
                .Include(c => c.Staff) // Eagerly load Staff
                .Include(c => c.Consultant) // Eagerly load Consultant
                .Where(c => c.CourseId == courseId)
                .Select(c => new CourseDTO
                {
                    CourseId = c.CourseId,
                    TopicId = c.TopicId,
                    CourseName = c.CourseName,
                    CourseType = c.CourseType,
                    StaffId = c.StaffId,
                    Description = c.Description, // Include Description
                    ConsultantId = c.ConsultantId, // Include ConsultantId
                    // ImageUrl and ImageUrl2 are no longer generated here
                    CreatedDate = c.CreatedDate,
                    Status = c.Status,
                    Inventory = c.Inventory,
                    IsSelected = c.IsSelected,
                    Topic = c.Topic != null ? new CourseTopicDTO
                    {
                        TopicId = c.Topic.TopicId,
                        TopicName = c.Topic.TopicName
                    } : null,
                    Staff = c.Staff != null ? new UserDTO
                    {
                        UserId = c.Staff.UserId,
                        Username = c.Staff.Username,
                        DoB = c.Staff.DoB,
                        PhoneNumber = c.Staff.PhoneNumber,
                        Email = c.Staff.Email,
                        RoleId = c.Staff.RoleId
                    } : null,
                    Consultant = c.Consultant != null ? new UserDTO // Include Consultant DTO
                    {
                        UserId = c.Consultant.UserId,
                        Username = c.Consultant.Username,
                        DoB = c.Consultant.DoB,
                        PhoneNumber = c.Consultant.PhoneNumber,
                        Email = c.Consultant.Email,
                        ImageUrl = $"images/{c.Consultant.UserId}.jpg", // Keep this if Consultant image is still from API/server
                        RoleId = c.Consultant.RoleId
                    } : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<CourseDTO>> GetAllAsync()
        {
            return await _context.Course
                .Include(c => c.Topic) // Eagerly load Topic
                .Include(c => c.Staff) // Eagerly load Staff
                .Include(c => c.Consultant) // Eagerly load Consultant
                .Select(c => new CourseDTO
                {
                    CourseId = c.CourseId,
                    TopicId = c.TopicId,
                    CourseName = c.CourseName,
                    CourseType = c.CourseType,
                    StaffId = c.StaffId,
                    Description = c.Description, // Include Description
                    ConsultantId = c.ConsultantId, // Include ConsultantId
                    // ImageUrl and ImageUrl2 are no longer generated here
                    CreatedDate = c.CreatedDate,
                    Status = c.Status,
                    Inventory = c.Inventory,
                    IsSelected = c.IsSelected,
                    Topic = c.Topic != null ? new CourseTopicDTO
                    {
                        TopicId = c.Topic.TopicId,
                        TopicName = c.Topic.TopicName
                    } : null,
                    Staff = c.Staff != null ? new UserDTO
                    {
                        UserId = c.Staff.UserId,
                        Username = c.Staff.Username,
                        DoB = c.Staff.DoB,
                        PhoneNumber = c.Staff.PhoneNumber,
                        Email = c.Staff.Email,
                        RoleId = c.Staff.RoleId
                    } : null,
                    Consultant = c.Consultant != null ? new UserDTO // Include Consultant DTO
                    {
                        UserId = c.Consultant.UserId,
                        Username = c.Consultant.Username,
                        DoB = c.Consultant.DoB,
                        PhoneNumber = c.Consultant.PhoneNumber,
                        Email = c.Consultant.Email,
                        ImageUrl = $"images/{c.Consultant.UserId}.jpg", // Keep this if Consultant image is still from API/server
                        RoleId = c.Consultant.RoleId
                    } : null
                })
                .ToListAsync();
        }

        public async Task<CourseDTO> UpdateAsync(Course course)
        {
            var existingCourse = await _context.Course.FindAsync(course.CourseId);
            if (existingCourse == null)
                throw new Exception($"Course with ID {course.CourseId} not found.");

            existingCourse.CourseName = course.CourseName;
            existingCourse.CourseType = course.CourseType;
            existingCourse.TopicId = course.TopicId;
            existingCourse.StaffId = course.StaffId;
            existingCourse.Description = course.Description; // Update Description
            existingCourse.ConsultantId = course.ConsultantId; // Update ConsultantId

            await _context.SaveChangesAsync();
            return new CourseDTO
            {
                CourseId = existingCourse.CourseId,
                TopicId = existingCourse.TopicId,
                CourseName = existingCourse.CourseName,
                CourseType = existingCourse.CourseType,
                StaffId = existingCourse.StaffId,
                Description = existingCourse.Description, // Include updated Description
                ConsultantId = existingCourse.ConsultantId, // Include updated ConsultantId
                // ImageUrl and ImageUrl2 are no longer generated here
                CreatedDate = existingCourse.CreatedDate,
                Status = existingCourse.Status,
                Inventory = existingCourse.Inventory,
                IsSelected = existingCourse.IsSelected
            };
        }

        public async Task<bool> DeleteAsync(string courseId)
        {
            var course = await _context.Course.FindAsync(courseId);
            if (course == null)
                return false;

            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<int> CountAsync()
        {
            return await _context.Course.CountAsync();
        }
    }
}
