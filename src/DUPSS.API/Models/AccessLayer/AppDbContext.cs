using DUPSS.API.Models.Objects;
using Microsoft.EntityFrameworkCore;

namespace DUPSS.API.Models.AccessLayer
{

    public class AppDbContext : DbContext
    {
        public DbSet<Role> Role { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Appointment> Appointment { get; set; }
        public DbSet<Campaign> Campaign { get; set; }
        public DbSet<CourseTopic> CourseTopic { get; set; }
        public DbSet<Course> Course { get; set; } // Make sure Course class has ImageUrl property
        public DbSet<CourseEnroll> CourseEnroll { get; set; }
        public DbSet<Assessment> Assessment { get; set; }
        public DbSet<AssessmentResult> AssessmentResult { get; set; }
        public DbSet<AssessmentQuestion> AssessmentQuestion { get; set; }
        public DbSet<AssessmentAnswer> AssessmentAnswer { get; set; } 
        public DbSet<Blog> Blog { get; set; }
        public DbSet<BlogTopic> BlogTopic { get; set; }

        public DbSet<CampaignRegistration> CampaignRegistrations { get; set; }

        public DbSet<Appointment> Appointments { get; set; } = null!;



        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tell Entity Framework Core to ignore the ImageUrl property on the Course entity
            // This is crucial because ImageUrl is dynamically set in the DAO, not stored in the DB.
            modelBuilder.Entity<Course>()
                .Ignore(c => c.ImageUrl); // <--- ADDED THIS LINE

            modelBuilder.Entity<Course>()
               .Ignore(c => c.ImageUrl2); // For consultant picture in course detail

            modelBuilder.Entity<Campaign>()
               .Ignore(c => c.ImageUrl);

            modelBuilder.Entity<User>()
               .Ignore(u => u.ImageUrl);

            // Role -> Users (one-to-many)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            // Users -> Appointments (member and consultant, one-to-many)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Member)
                .WithMany(u => u.MemberAppointments)
                .HasForeignKey(a => a.MemberId);
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Consultant)
                .WithMany(u => u.ConsultantAppointments)
                .HasForeignKey(a => a.ConsultantId);

            // Users -> Campaigns (one-to-many)
            modelBuilder.Entity<Campaign>()
                .HasOne(c => c.Staff)
                .WithMany(u => u.Campaigns)
                .HasForeignKey(c => c.StaffId);


            // CourseTopic -> Courses (one-to-many)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Topic)
                .WithMany(t => t.Courses)
                .HasForeignKey(c => c.TopicId);

            // Users -> Courses (one-to-many)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Staff)
                .WithMany(u => u.Courses)
                .HasForeignKey(c => c.StaffId);

            // Users -> CourseEnroll (one-to-many)
            modelBuilder.Entity<CourseEnroll>()
                .HasOne(ce => ce.Member)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(ce => ce.MemberId);

            // Course -> CourseEnroll (one-to-many)
            modelBuilder.Entity<CourseEnroll>()
                .HasOne(ce => ce.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(ce => ce.CourseId);

            // Users -> Blogs (one-to-many)
            modelBuilder.Entity<Blog>()
                .HasOne(b => b.Staff)
                .WithMany(u => u.Blogs)
                .HasForeignKey(b => b.StaffId);

            // BlogTopic -> Blogs (one-to-many)
            modelBuilder.Entity<Blog>()
                .HasOne(b => b.BlogTopic)
                .WithMany(bt => bt.Blogs)
                .HasForeignKey(b => b.BlogTopicId);

            modelBuilder.Entity<CampaignRegistration>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.MemberId);

            modelBuilder.Entity<CampaignRegistration>()
                .HasOne(r => r.Campaign)
                .WithMany()
                .HasForeignKey(r => r.CampaignId);

            // Assessment -> AssessmentResult (one-to-many)
            modelBuilder.Entity<AssessmentResult>()
                .HasOne(ar => ar.Assessment)
                .WithMany(a => a.Results)
                .HasForeignKey(ar => ar.AssessmentId);

            // Users -> AssessmentResult (one-to-many)
            modelBuilder.Entity<AssessmentResult>()
                .HasOne(ar => ar.Member)
                .WithMany(u => u.AssessmentResults)
                .HasForeignKey(ar => ar.MemberId);

            // Assessment -> AssessmentQuestion (one-to-many)
            modelBuilder.Entity<AssessmentQuestion>()
                .HasOne(a => a.Assessment)
                .WithMany(a => a.Questions)
                .HasForeignKey(q => q.AssessmentId);

            // AssessmentQuestion -> AssessmentAnswer (one-to-many)
            modelBuilder.Entity<AssessmentAnswer>()
                .HasOne(q => q.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId);


            // Configure indexes (optional, as schema already defines them)
            modelBuilder.Entity<User>().HasIndex(u => u.RoleId);
            modelBuilder.Entity<Appointment>().HasIndex(a => a.MemberId);
            modelBuilder.Entity<Appointment>().HasIndex(a => a.ConsultantId);
            modelBuilder.Entity<Campaign>().HasIndex(c => c.StaffId);
            modelBuilder.Entity<Course>().HasIndex(c => c.TopicId);
            modelBuilder.Entity<Course>().HasIndex(c => c.StaffId);
            modelBuilder.Entity<CourseEnroll>().HasIndex(ce => ce.MemberId);
            modelBuilder.Entity<CourseEnroll>().HasIndex(ce => ce.CourseId);
            modelBuilder.Entity<AssessmentResult>().HasIndex(ar => ar.AssessmentId);
            modelBuilder.Entity<AssessmentResult>().HasIndex(ar => ar.MemberId);
            modelBuilder.Entity<Blog>().HasIndex(b => b.StaffId);
            modelBuilder.Entity<BlogTopic>().HasIndex(bt => bt.BlogTopicId);

        }
    }
}
