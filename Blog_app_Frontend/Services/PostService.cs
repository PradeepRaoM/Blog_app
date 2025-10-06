using Blog_app_Frontend.Models;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Blog_app_Frontend.Services
{
    public class PostService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public PostService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        // ---------------- POSTS ----------------

        public async Task<List<PostDto>> GetAllPostsAsync()
        {
            return await _http.GetFromJsonAsync<List<PostDto>>("api/posts") ?? new List<PostDto>();
        }

        public async Task<List<PostDto>> GetPublishedPostsAsync()
        {
            return await _http.GetFromJsonAsync<List<PostDto>>("api/posts/published") ?? new List<PostDto>();
        }

        public async Task<PostDto?> GetPostByIdAsync(Guid id)
        {
            return await _http.GetFromJsonAsync<PostDto>($"api/posts/{id}");
        }

        public async Task<PostInsightsDto?> GetPostInsightsAsync(Guid postId)
        {
            return await _http.GetFromJsonAsync<PostInsightsDto>($"api/posts/{postId}/insights");
        }

        public async Task<List<PostDto>> SearchPostsAsync(string query, int page = 1, int limit = 10)
        {
            return await _http.GetFromJsonAsync<List<PostDto>>(
                $"api/posts/search?q={Uri.EscapeDataString(query)}&page={page}&limit={limit}") ?? new List<PostDto>();
        }

        public async Task<List<PostDto>> GetFeedAsync(int page = 1, int limit = 10)
        {
            return await _http.GetFromJsonAsync<List<PostDto>>($"api/posts/feed?page={page}&limit={limit}") ?? new List<PostDto>();
        }

        public async Task<PostDto> CreateOrUpdatePostAsync(CreatePostDto dto, bool isUpdate = false, Guid? postId = null)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(dto.Title ?? ""), nameof(dto.Title));
            content.Add(new StringContent(dto.ContentMarkdown ?? ""), nameof(dto.ContentMarkdown));

            var slug = !string.IsNullOrWhiteSpace(dto.Title)
                        ? dto.Title.Trim().ToLower().Replace(" ", "-")
                        : Guid.NewGuid().ToString();
            content.Add(new StringContent(slug), "Slug");

            var metaTitle = !string.IsNullOrWhiteSpace(dto.Title) ? dto.Title : "Untitled Post";
            content.Add(new StringContent(metaTitle), "MetaTitle");

            var metaDescription = !string.IsNullOrWhiteSpace(dto.ContentMarkdown)
                                  ? dto.ContentMarkdown.Substring(0, Math.Min(150, dto.ContentMarkdown.Length))
                                  : "";
            content.Add(new StringContent(metaDescription), "MetaDescription");

            if (!string.IsNullOrEmpty(dto.Status))
                content.Add(new StringContent(dto.Status), nameof(dto.Status));

            if (dto.CategoryId != null)
                content.Add(new StringContent(dto.CategoryId.ToString()), nameof(dto.CategoryId));

            if (dto.ScheduledFor != null)
                content.Add(new StringContent(dto.ScheduledFor.Value.ToString("o")), nameof(dto.ScheduledFor));

            if (!string.IsNullOrEmpty(dto.LocationTag))
                content.Add(new StringContent(dto.LocationTag), nameof(dto.LocationTag));

            if (dto.TagIds != null && dto.TagIds.Count > 0)
                foreach (var tagId in dto.TagIds)
                    content.Add(new StringContent(tagId.ToString()), "TagIds");

            if (dto.Hashtags != null && dto.Hashtags.Count > 0)
                foreach (var tag in dto.Hashtags)
                    content.Add(new StringContent(tag), "Hashtags");

            if (dto.MentionedUserIds != null && dto.MentionedUserIds.Count > 0)
                foreach (var id in dto.MentionedUserIds)
                    content.Add(new StringContent(id.ToString()), "MentionedUserIds");

            if (dto.FeaturedImageFile != null)
            {
                var streamContent = new StreamContent(dto.FeaturedImageFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(dto.FeaturedImageFile.ContentType);
                content.Add(streamContent, "featuredImage", dto.FeaturedImageFile.Name);
            }

            HttpResponseMessage response;
            if (isUpdate && postId != null)
                response = await _http.PutAsync($"api/posts/{postId}", content);
            else
                response = await _http.PostAsync("api/posts", content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to save post: {err}");
            }

            return await response.Content.ReadFromJsonAsync<PostDto>();
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var response = await _http.DeleteAsync($"api/posts/{postId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<UserPostsResponse?> GetProfileWithPostsAsync(Guid userId)
        {
            return await _http.GetFromJsonAsync<UserPostsResponse>($"api/Profile/{userId}");
        }

        public async Task<MyUserPostsResponse?> GetMyProfileWithPostsAsync()
        {
            return await _http.GetFromJsonAsync<MyUserPostsResponse>("api/Profile/me");
        }

        // ---------------- ARCHIVE ----------------
        public async Task<Dictionary<string, List<PostDto>>> GetArchiveAsync()
        {
            return await _http.GetFromJsonAsync<Dictionary<string, List<PostDto>>>("api/posts/archive")
                   ?? new Dictionary<string, List<PostDto>>();
        }

        // ---------------- RELATED POSTS ----------------
        public async Task<List<PostDto>> GetRelatedPostsAsync(Guid postId)
        {
            return await _http.GetFromJsonAsync<List<PostDto>>($"api/posts/{postId}/related") ?? new List<PostDto>();
        }

        // ---------------- FILTER OPTIONS ----------------
        public async Task<List<PostDto>> GetPostsByFilterAsync(string filterType, string filterValue)
        {
            return await _http.GetFromJsonAsync<List<PostDto>>(
                $"api/posts/filter?filterType={Uri.EscapeDataString(filterType)}&filterValue={Uri.EscapeDataString(filterValue)}")
                ?? new List<PostDto>();
        }

        public async Task<FilterOptionsDto?> GetFilterOptionsAsync()
        {
            return await _http.GetFromJsonAsync<FilterOptionsDto>("api/posts/filters");
        }
    }

    // ---------------- RESPONSE MODELS ----------------
    public class UserPostsResponse
    {
        public Profile Profile { get; set; } = new();
        public List<PostDto> Posts { get; set; } = new();
    }

    public class MyUserPostsResponse
    {
        public Profile Profile { get; set; } = new();
        public Dictionary<string, List<PostDto>> Posts { get; set; } = new();
    }
}
