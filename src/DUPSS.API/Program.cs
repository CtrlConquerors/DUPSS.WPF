using DUPSS.API.Models.AccessLayer;
using DUPSS.API.Models.AccessLayer.DAOs;
using DUPSS.API.Models.AccessLayer.Interfaces;
using DUPSS.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add DbContext
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthorization();

// Add DAOs
builder.Services.AddScoped<IRoleDAO, RoleDAO>();
builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<IAppointmentDAO, AppointmentDAO>();
builder.Services.AddScoped<ICampaignDAO, CampaignDAO>();
builder.Services.AddScoped<ICourseTopicDAO, CourseTopicDAO>();
builder.Services.AddScoped<ICourseDAO, CourseDAO>();
builder.Services.AddScoped<ICourseEnrollDAO, CourseEnrollDAO>();
builder.Services.AddScoped<IAssessmentDAO, AssessmentDAO>();
builder.Services.AddScoped<IAssessmentResultDAO, AssessmentResultDAO>();
builder.Services.AddScoped<IAssessmentQuestionDAO, AssessmentQuestionDAO>();
builder.Services.AddScoped<IAssessmentAnswerDAO, AssessmentAnswerDAO>();
builder.Services.AddScoped<IBlogDAO, BlogDAO>();
builder.Services.AddScoped<IBlogTopicDAO, BlogTopicDAO>();
builder.Services.AddScoped<ICampaignRegistrationDAO, CampaignRegistrationDAO>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", builder =>
        builder.WithOrigins("https://localhost:7084")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();


app.UseCors("AllowBlazor");
app.MapOpenApi();
app.MapScalarApiReference();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
