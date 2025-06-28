using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Models.DTOs;
using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;

namespace DUPSS.API.Models.AccessLayer.DAOs
{
    public class BlogDAO : IBlogDAO
    {
        private readonly AppDbContext _context;

        public BlogDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BlogDTO> CreateAsync(Blog blog)
        {
            _context.Blog.Add(blog);
            await _context.SaveChangesAsync();
            return new BlogDTO
            {
                BlogId = blog.BlogId,
                StaffId = blog.StaffId,
                Title = blog.Title,
                Content = blog.Content,
                Status = blog.Status,
                BlogTopicId = blog.BlogTopicId,
            };
        }

        public async Task<BlogDTO?> GetByIdAsync(string blogId)
        {
            return await _context.Blog
                .Where(b => b.BlogId == blogId)
                .Select(b => new BlogDTO
                {
                    BlogId = b.BlogId,
                    StaffId = b.StaffId,
                    Title = b.Title,
                    Content = b.Content,
                    Status = b.Status,
                    BlogTopicId = b.BlogTopicId,
                    Staff = b.Staff != null ? new UserDTO
                    {
                        UserId = b.Staff.UserId,
                        Username = b.Staff.Username,
                        DoB = b.Staff.DoB,
                        PhoneNumber = b.Staff.PhoneNumber,
                        Email = b.Staff.Email,
                        RoleId = b.Staff.RoleId
                    } : null,
                    BlogTopic = b.BlogTopic != null ? new BlogTopicDTO
                    {
                        BlogTopicId = b.BlogTopic.BlogTopicId,
                        BlogTopicName = b.BlogTopic.BlogTopicName,
                    } : null,
                    ImageUrl = $"/images/{b.BlogId}.jpg"
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<BlogDTO>> GetAllAsync()
        {
            return await _context.Blog
                .Select(b => new BlogDTO
                {
                    BlogId = b.BlogId,
                    StaffId = b.StaffId,
                    Title = b.Title,
                    Content = b.Content,
                    Status = b.Status,
                    BlogTopicId = b.BlogTopicId,
                    Staff = b.Staff != null ? new UserDTO
                    {
                        UserId = b.Staff.UserId,
                        Username = b.Staff.Username,
                        DoB = b.Staff.DoB,
                        PhoneNumber = b.Staff.PhoneNumber,
                        Email = b.Staff.Email,
                        RoleId = b.Staff.RoleId
                    } : null,
                    BlogTopic = b.BlogTopic != null ? new BlogTopicDTO
                    {
                        BlogTopicId = b.BlogTopic.BlogTopicId,
                        BlogTopicName = b.BlogTopic.BlogTopicName,
                    } : null,
                    ImageUrl = $"/images/{b.BlogId}.jpg"
                })
                .ToListAsync();
        }

        public async Task<BlogDTO> UpdateAsync(Blog blog)
        {
            var existingBlog = await _context.Blog.FindAsync(blog.BlogId);
            if (existingBlog == null)
                throw new Exception($"Blog with ID {blog.BlogId} not found.");

            existingBlog.StaffId = blog.StaffId;
            existingBlog.Title = blog.Title;
            existingBlog.Content = blog.Content;
            existingBlog.Status = blog.Status;
            existingBlog.BlogTopicId = blog.BlogTopicId;

            await _context.SaveChangesAsync();
            return new BlogDTO
            {
                BlogId = existingBlog.BlogId,
                StaffId = existingBlog.StaffId,
                Title = existingBlog.Title,
                Content = existingBlog.Content,
                Status = existingBlog.Status,
                BlogTopicId = existingBlog.BlogTopicId,
            };
        }

        public async Task<bool> DeleteAsync(string blogId)
        {
            var blog = await _context.Blog.FindAsync(blogId);
            if (blog == null)
                return false;

            _context.Blog.Remove(blog);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
