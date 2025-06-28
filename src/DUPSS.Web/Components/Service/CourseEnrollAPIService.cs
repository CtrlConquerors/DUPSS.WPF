using DUPSS.API.Models.DTOs;
using System; // Added for Console.WriteLine and Exception
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DUPSS.Web.Components.Service
{
    // Inherit from GenericApiService<CourseEnrollDTO>
    public class CourseEnrollApiService : GenericApiService<CourseEnrollDTO>
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/CourseEnrolls"; // This should match your API controller's route

        public CourseEnrollApiService(HttpClient httpClient)
            : base(httpClient, BaseUrl) // Pass dependencies to the base class
        {
            _httpClient = httpClient;
        }

        // All standard CRUD methods (GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync)
        // are now inherited from GenericApiService<T> and do not need to be duplicated here.


        public async Task<List<CourseEnrollDTO>?> GetEnrollmentsByMemberAndCourse(string memberId, string courseId)
        {
            try
            {
                // Ensure authorization header is set for this custom call as well
                //await AddAuthorizationHeader();

                // Construct the query string for memberId and courseId
                var response = await _httpClient.GetAsync($"{BaseUrl}/GetByMemberAndCourse?memberId={memberId}&courseId={courseId}");
                response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error code

                return await response.Content.ReadFromJsonAsync<List<CourseEnrollDTO>>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching enrollments for member {memberId} and course {courseId}: {ex.Message}");
                // In a real application, you might want to log this with ILogger
                throw;
            }
        }
        public async Task<int> GetCountAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<int>("api/CourseEnrolls/Count");
            return result;
        }
        /// <summary>
        /// Adds the authorization header to HttpClient default request headers if a token exists.
        /// This method is duplicated from GenericApiService because custom methods in derived
        /// services need access to set headers if they make direct HttpClient calls.
        /// </summary>
        //private async Task AddAuthorizationHeader()
        //{
        //    var token = _authService.GetToken();
        //    if (!string.IsNullOrEmpty(token))
        //    {
        //        _httpClient.DefaultRequestHeaders.Authorization =
        //            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        //    }
        //    else
        //    {
        //        _httpClient.DefaultRequestHeaders.Authorization = null;
        //    }
        //}
    }
}
