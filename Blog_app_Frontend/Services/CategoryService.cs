using Blog_app_Frontend.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace Blog_app_Frontend.Services
{
    public class CategoryService
    {
        private readonly HttpClient _http;

        public CategoryService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            return await _http.GetFromJsonAsync<List<CategoryDto>>("https://localhost:7247/api/categories");
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
        {
            return await _http.GetFromJsonAsync<CategoryDto>($"https://localhost:7247/api/categories/{id}");
        }

        public async Task<bool> CreateCategoryAsync(CategoryDto category)
        {
            var response = await _http.PostAsJsonAsync("https://localhost:7247/api/categories", category);
            return response.IsSuccessStatusCode;
        }
        public async Task DeleteCategoryAsync(Guid id)
        {
            await _http.DeleteAsync($"https://localhost:7247/api/categories/{id}");
        }


    }
}
