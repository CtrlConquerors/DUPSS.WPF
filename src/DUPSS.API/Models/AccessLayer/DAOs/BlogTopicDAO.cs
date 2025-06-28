using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;

namespace DUPSS.API.Models.AccessLayer.DAOs
{
    public class BlogTopicDAO : IBlogTopicDAO
    {
        private readonly AppDbContext _context;
        
        public BlogTopicDAO(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<BlogTopicDTO> CreateAsync(BlogTopic blogTopic)
        {
            _context.BlogTopic.Add(blogTopic);
            await _context.SaveChangesAsync();
            return new BlogTopicDTO
            {
                BlogTopicId = blogTopic.BlogTopicId,
                BlogTopicName = blogTopic.BlogTopicName
            };
        }

        public async Task<List<BlogTopicDTO>> GetAllAsync()
        {
            return await _context.BlogTopic
                .Select(bt => new BlogTopicDTO
                {
                    BlogTopicId = bt.BlogTopicId,
                    BlogTopicName = bt.BlogTopicName
                })
                .ToListAsync();
        }

        public async Task<BlogTopicDTO?> GetByIdAsync(string blogTopicId)
        {
            return await _context.BlogTopic
                .Where(bt => bt.BlogTopicId == blogTopicId)
                .Select(bt => new BlogTopicDTO
                {
                    BlogTopicId = bt.BlogTopicId,
                    BlogTopicName = bt.BlogTopicName
                })
                .FirstOrDefaultAsync();
        }

        public async Task<BlogTopicDTO> UpdateAsync(BlogTopic blogTopic)
        {
            var existingBlogTopic = await _context.BlogTopic.FindAsync(blogTopic.BlogTopicId);
            if (existingBlogTopic == null)
                throw new Exception($"BlogTopic with ID {blogTopic.BlogTopicId} not found.");

            existingBlogTopic.BlogTopicName = blogTopic.BlogTopicName;

            await _context.SaveChangesAsync();
            return new BlogTopicDTO
            {
                BlogTopicId = existingBlogTopic.BlogTopicId,
                BlogTopicName = existingBlogTopic.BlogTopicName,
            };
        }

        public async Task<bool> DeleteAsync(string blogTopicId)
        {
            var blogTopic = await _context.BlogTopic.FindAsync(blogTopicId);
            if (blogTopic == null)
                return false;

            _context.BlogTopic.Remove(blogTopic);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}