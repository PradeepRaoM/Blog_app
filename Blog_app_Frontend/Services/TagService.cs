using Blog_app_Frontend.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Blog_app_Frontend.Services
{
    public class TagService
    {
        private readonly HttpClient _http;

        public TagService(HttpClient http)
        {
            _http = http;
        }

        // Get all tags
        public async Task<List<TagDto>> GetTagsAsync()
        {
            return await _http.GetFromJsonAsync<List<TagDto>>("api/tags");
        }

        // Create a new tag
        public async Task<TagDto> CreateTagAsync(TagDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/tags", dto);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<TagDto>();
        }

        // Delete a tag
        public async Task<bool> DeleteTagAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"api/tags/{id}");
            return response.IsSuccessStatusCode;
        }

        // Get tag by ID (optional)
        public async Task<TagDto> GetTagByIdAsync(Guid id)
        {
            return await _http.GetFromJsonAsync<TagDto>($"api/tags/{id}");
        }
    }
}
