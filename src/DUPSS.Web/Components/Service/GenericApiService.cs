namespace DUPSS.Web.Components.Service
{
    public class GenericApiService<T> where T : class
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;

        public GenericApiService(HttpClient httpClient, string endpoint)
        {
            _httpClient = httpClient;
            _endpoint = endpoint.TrimEnd('/');
        }

        public async Task<List<T>> GetAllAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<List<T>>($"{_endpoint}/GetAll");
            return result ?? new List<T>();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await _httpClient.GetFromJsonAsync<T>($"{_endpoint}/GetById/{id}");
        }

        public async Task<T?> CreateAsync(T entity)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_endpoint}/Create", entity);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            return null;
        }

        public async Task<T?> UpdateAsync(T entity)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_endpoint}/Update", entity);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            return null;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"{_endpoint}/Delete/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}