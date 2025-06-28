using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;
namespace DUPSS.API.Models.AccessLayer.DAOs
{
    public class CourseTopicDAO : ICourseTopicDAO
    {
        private readonly AppDbContext _context;

        public CourseTopicDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CourseTopicDTO> CreateAsync(CourseTopic courseTopic)
        {
            _context.CourseTopic.Add(courseTopic);
            await _context.SaveChangesAsync();
            return new CourseTopicDTO
            {
                TopicId = courseTopic.TopicId,
                TopicName = courseTopic.TopicName
            };
        }

        public async Task<CourseTopicDTO?> GetByIdAsync(string topicId)
        {
            return await _context.CourseTopic
                .Where(ct => ct.TopicId == topicId)
                .Select(ct => new CourseTopicDTO
                {
                    TopicId = ct.TopicId,
                    TopicName = ct.TopicName
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<CourseTopicDTO>> GetAllAsync()
        {
            return await _context.CourseTopic
                .Select(ct => new CourseTopicDTO
                {
                    TopicId = ct.TopicId,
                    TopicName = ct.TopicName
                })
                .ToListAsync();
        }

        public async Task<CourseTopicDTO> UpdateAsync(CourseTopic courseTopic)
        {
            var existingTopic = await _context.CourseTopic.FindAsync(courseTopic.TopicId);
            if (existingTopic == null)
                throw new Exception($"CourseTopic with ID {courseTopic.TopicId} not found.");

            existingTopic.TopicName = courseTopic.TopicName;

            await _context.SaveChangesAsync();
            return new CourseTopicDTO
            {
                TopicId = existingTopic.TopicId,
                TopicName = existingTopic.TopicName
            };
        }

        public async Task<bool> DeleteAsync(string topicId)
        {
            var courseTopic = await _context.CourseTopic.FindAsync(topicId);
            if (courseTopic == null)
                return false;

            _context.CourseTopic.Remove(courseTopic);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
