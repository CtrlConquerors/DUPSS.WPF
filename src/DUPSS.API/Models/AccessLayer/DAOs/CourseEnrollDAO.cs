using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq; // Added for .Where() and .Any()

namespace DUPSS.API.Models.AccessLayer.DAOs
{
    public class CourseEnrollDAO : ICourseEnrollDAO
    {
        // Change from AppDbContext to IDbContextFactory<AppDbContext>
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        // Constructor now takes IDbContextFactory
        public CourseEnrollDAO(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<CourseEnrollDTO> CreateAsync(CourseEnroll courseEnroll)
        {
            // Create a new DbContext instance for this operation and ensure it's disposed
            using (var _context = await _contextFactory.CreateDbContextAsync())
            {
                _context.CourseEnroll.Add(courseEnroll);
                await _context.SaveChangesAsync();
                // It's good practice to fetch the DTO with eager loaded related data if needed immediately after creation
                // However, for a simple creation response, this projection is sufficient.
                return new CourseEnrollDTO
                {
                    EnrollId = courseEnroll.EnrollId,
                    MemberId = courseEnroll.MemberId,
                    CourseId = courseEnroll.CourseId,
                    Status = courseEnroll.Status,
                    EnrollDate = courseEnroll.EnrollDate,
                    CompleteDate = courseEnroll.CompleteDate
                };
            }
        }

        public async Task<CourseEnrollDTO?> GetByIdAsync(string enrollId)
        {
            // Create a new DbContext instance for this operation and ensure it's disposed
            using (var _context = await _contextFactory.CreateDbContextAsync())
            {
                return await _context.CourseEnroll
                    .Include(ce => ce.Member) // Eagerly load Member
                    .Include(ce => ce.Course) // Eagerly load Course
                        .ThenInclude(c => c.Topic) // Then include Topic of the Course
                    .Include(ce => ce.Course) // Re-include Course to chain for Staff
                        .ThenInclude(c => c.Staff) // Then include Staff of the Course
                    .Include(ce => ce.Course) // Re-include Course to chain for Consultant
                        .ThenInclude(c => c.Consultant) // Include Consultant of the Course
                    .Where(ce => ce.EnrollId == enrollId)
                    .Select(ce => new CourseEnrollDTO
                    {
                        EnrollId = ce.EnrollId,
                        MemberId = ce.MemberId,
                        CourseId = ce.CourseId,
                        Status = ce.Status,
                        EnrollDate = ce.EnrollDate,
                        CompleteDate = ce.CompleteDate,
                        Member = ce.Member != null ? new UserDTO
                        {
                            UserId = ce.Member.UserId,
                            Username = ce.Member.Username,
                            DoB = ce.Member.DoB,
                            PhoneNumber = ce.Member.PhoneNumber,
                            Email = ce.Member.Email,
                            RoleId = ce.Member.RoleId
                        } : null,
                        Course = ce.Course != null ? new CourseDTO
                        {
                            CourseId = ce.Course.CourseId,
                            TopicId = ce.Course.TopicId,
                            CourseName = ce.Course.CourseName,
                            CourseType = ce.Course.CourseType,
                            StaffId = ce.Course.StaffId,
                            Description = ce.Course.Description,
                            ConsultantId = ce.Course.ConsultantId,
                            ImageUrl = $"images/{ce.Course.CourseId}.jpg", // This assumes images are static files
                            ImageUrl2 = $"images/{ce.Course.ConsultantId}.jpg", // Added ImageUrl2 for consultant picture
                            CreatedDate = ce.Course.CreatedDate,
                            Status = ce.Course.Status,
                            Inventory = ce.Course.Inventory,
                            IsSelected = ce.Course.IsSelected,
                            Topic = ce.Course.Topic != null ? new CourseTopicDTO
                            {
                                TopicId = ce.Course.Topic.TopicId,
                                TopicName = ce.Course.Topic.TopicName
                            } : null,
                            Staff = ce.Course.Staff != null ? new UserDTO
                            {
                                UserId = ce.Course.Staff.UserId,
                                Username = ce.Course.Staff.Username,
                                DoB = ce.Course.Staff.DoB,
                                PhoneNumber = ce.Course.Staff.PhoneNumber,
                                Email = ce.Course.Staff.Email,
                                RoleId = ce.Course.Staff.RoleId
                            } : null,
                            Consultant = ce.Course.Consultant != null ? new UserDTO
                            {
                                UserId = ce.Course.Consultant.UserId,
                                Username = ce.Course.Consultant.Username,
                                DoB = ce.Course.Consultant.DoB,
                                PhoneNumber = ce.Course.Consultant.PhoneNumber,
                                Email = ce.Course.Consultant.Email,
                                RoleId = ce.Course.Consultant.RoleId
                            } : null
                        } : null
                    })
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<List<CourseEnrollDTO>> GetAllAsync()
        {
            // Create a new DbContext instance for this operation and ensure it's disposed
            using (var _context = await _contextFactory.CreateDbContextAsync())
            {
                return await _context.CourseEnroll
                    .Include(ce => ce.Member) // Eagerly load Member
                    .Include(ce => ce.Course) // Eagerly load Course
                        .ThenInclude(c => c.Topic) // Then include Topic of the Course
                    .Include(ce => ce.Course) // Re-include Course to chain for Staff
                        .ThenInclude(c => c.Staff) // Then include Staff of the Course
                    .Include(ce => ce.Course) // Re-include Course to chain for Consultant
                        .ThenInclude(c => c.Consultant) // Include Consultant of the Course
                    .Select(ce => new CourseEnrollDTO
                    {
                        EnrollId = ce.EnrollId,
                        MemberId = ce.MemberId,
                        CourseId = ce.CourseId,
                        Status = ce.Status,
                        EnrollDate = ce.EnrollDate,
                        CompleteDate = ce.CompleteDate,
                        Member = ce.Member != null ? new UserDTO
                        {
                            UserId = ce.Member.UserId,
                            Username = ce.Member.Username,
                            DoB = ce.Member.DoB,
                            PhoneNumber = ce.Member.PhoneNumber,
                            Email = ce.Member.Email,
                            RoleId = ce.Member.RoleId
                        } : null,
                        Course = ce.Course != null ? new CourseDTO
                        {
                            CourseId = ce.Course.CourseId,
                            TopicId = ce.Course.TopicId,
                            CourseName = ce.Course.CourseName,
                            CourseType = ce.Course.CourseType,
                            StaffId = ce.Course.StaffId,
                            Description = ce.Course.Description,
                            ConsultantId = ce.Course.ConsultantId,
                            ImageUrl = $"images/{ce.Course.CourseId}.jpg",
                            ImageUrl2 = $"images/{ce.Course.ConsultantId}.jpg", // Added ImageUrl2 for consultant picture
                            CreatedDate = ce.Course.CreatedDate,
                            Status = ce.Course.Status,
                            Inventory = ce.Course.Inventory,
                            IsSelected = ce.Course.IsSelected,
                            Topic = ce.Course.Topic != null ? new CourseTopicDTO
                            {
                                TopicId = ce.Course.Topic.TopicId,
                                TopicName = ce.Course.Topic.TopicName
                            } : null,
                            Staff = ce.Course.Staff != null ? new UserDTO
                            {
                                UserId = ce.Course.Staff.UserId,
                                Username = ce.Course.Staff.Username,
                                DoB = ce.Course.Staff.DoB,
                                PhoneNumber = ce.Course.Staff.PhoneNumber,
                                Email = ce.Course.Staff.Email,
                                RoleId = ce.Course.Staff.RoleId
                            } : null,
                            Consultant = ce.Course.Consultant != null ? new UserDTO
                            {
                                UserId = ce.Course.Consultant.UserId,
                                Username = ce.Course.Consultant.Username,
                                DoB = ce.Course.Consultant.DoB,
                                PhoneNumber = ce.Course.Consultant.PhoneNumber,
                                Email = ce.Course.Consultant.Email,
                                RoleId = ce.Course.Consultant.RoleId
                            } : null
                        } : null
                    })
                    .ToListAsync();
            }
        }

        public async Task<CourseEnrollDTO> UpdateAsync(CourseEnroll courseEnroll)
        {
            // Create a new DbContext instance for this operation and ensure it's disposed
            using (var _context = await _contextFactory.CreateDbContextAsync())
            {
                var existingEnroll = await _context.CourseEnroll.FindAsync(courseEnroll.EnrollId);
                if (existingEnroll == null)
                    throw new Exception($"CourseEnroll with ID {courseEnroll.EnrollId} not found.");

                existingEnroll.MemberId = courseEnroll.MemberId;
                existingEnroll.CourseId = courseEnroll.CourseId;
                existingEnroll.Status = courseEnroll.Status;
                existingEnroll.EnrollDate = courseEnroll.EnrollDate;
                existingEnroll.CompleteDate = courseEnroll.CompleteDate;

                await _context.SaveChangesAsync();
                // It's good practice to return the DTO with eager loaded related data after update
                // if the client needs to immediately reflect those changes.
                // For now, mirroring the input properties is fine if the client updates its local state.
                return new CourseEnrollDTO
                {
                    EnrollId = existingEnroll.EnrollId,
                    MemberId = existingEnroll.MemberId,
                    CourseId = existingEnroll.CourseId,
                    Status = existingEnroll.Status,
                    EnrollDate = existingEnroll.EnrollDate,
                    CompleteDate = existingEnroll.CompleteDate
                };
            }
        }

        public async Task<bool> DeleteAsync(string enrollId)
        {
            // Create a new DbContext instance for this operation and ensure it's disposed
            using (var _context = await _contextFactory.CreateDbContextAsync())
            {
                var courseEnroll = await _context.CourseEnroll.FindAsync(enrollId);
                if (courseEnroll == null)
                    return false;

                _context.CourseEnroll.Remove(courseEnroll);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        public async Task<int> CountAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.CourseEnroll.CountAsync();
        }
        /// <summary>
        /// Retrieves course enrollments for a specific member in a specific course.
        /// Used primarily to check for duplicate enrollments before creation.
        /// </summary>
        /// <param name="memberId">The ID of the member.</param>
        /// <param name="courseId">The ID of the course.</param>
        /// <returns>A list of CourseEnrollDTOs matching the criteria.</returns>
        public async Task<List<CourseEnrollDTO>?> GetEnrollmentsByMemberAndCourse(string memberId, string courseId)
        {
            // Create a new DbContext instance for this operation and ensure it's disposed
            using (var _context = await _contextFactory.CreateDbContextAsync())
            {
                return await _context.CourseEnroll
                    .Include(ce => ce.Member)
                    .Include(ce => ce.Course)
                    .Where(ce => ce.MemberId == memberId && ce.CourseId == courseId)
                    .Select(ce => new CourseEnrollDTO
                    {
                        EnrollId = ce.EnrollId,
                        MemberId = ce.MemberId,
                        CourseId = ce.Course.CourseId, // Ensure CourseId from the actual Course object
                        Status = ce.Status, // Assuming status is relevant for existing check
                        EnrollDate = ce.EnrollDate, // Assuming enrollDate is relevant for existing check
                        CompleteDate = ce.CompleteDate, // Assuming completeDate is relevant for existing check
                        Member = ce.Member != null ? new UserDTO
                        {
                            UserId = ce.Member.UserId,
                            Username = ce.Member.Username,
                            Email = ce.Member.Email,
                            RoleId = ce.Member.RoleId
                        } : null,
                        Course = ce.Course != null ? new CourseDTO
                        {
                            CourseId = ce.Course.CourseId,
                            CourseName = ce.Course.CourseName,
                            TopicId = ce.Course.TopicId,       // Added: Required property
                            CourseType = ce.Course.CourseType, // Added: Required property
                            StaffId = ce.Course.StaffId,       // Added: Required property
                            ConsultantId = ce.Course.ConsultantId, // Added: Required property
                            Description = ce.Course.Description,
                            ImageUrl = $"images/{ce.Course.CourseId}.jpg",
                            ImageUrl2 = $"images/{ce.Course.ConsultantId}.jpg",
                            CreatedDate = ce.Course.CreatedDate,
                            Status = ce.Course.Status,
                            Inventory = ce.Course.Inventory,
                            IsSelected = ce.Course.IsSelected,
                            Topic = ce.Course.Topic != null ? new CourseTopicDTO { TopicId = ce.Course.Topic.TopicId, TopicName = ce.Course.Topic.TopicName } : null,
                            Staff = ce.Course.Staff != null ? new UserDTO
                            {
                                UserId = ce.Course.Staff.UserId,
                                Username = ce.Course.Staff.Username,
                                Email = ce.Course.Staff.Email, // Ensure Email is populated
                                RoleId = ce.Course.Staff.RoleId // Ensure RoleId is populated
                            } : null,
                            Consultant = ce.Course.Consultant != null ? new UserDTO
                            {
                                UserId = ce.Course.Consultant.UserId,
                                Username = ce.Course.Consultant.Username,
                                Email = ce.Course.Consultant.Email, // Ensure Email is populated
                                RoleId = ce.Course.Consultant.RoleId // Ensure RoleId is populated
                            } : null
                        } : null
                    })
                    .ToListAsync();
            }
        }
    }
}
