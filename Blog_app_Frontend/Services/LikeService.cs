using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Threading.Tasks;

namespace Blog_app_Frontend.Services
{
    public class LikeService
    {
        private readonly HttpClient _http;

        public LikeService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Like a post
        /// </summary>
        public async Task<bool> LikeAsync(Guid postId)
        {
            var response = await _http.PostAsync($"api/likes/{postId}/like", null);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Dislike (remove like) a post
        /// </summary>
        public async Task<bool> DislikeAsync(Guid postId)
        {
            var response = await _http.PostAsync($"api/likes/{postId}/dislike", null);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Get total like count for a post
        /// </summary>
        public async Task<int> GetLikeCountAsync(Guid postId)
        {
            var result = await _http.GetFromJsonAsync<LikeCountResponse>($"api/likes/{postId}/count");
            return result?.Count ?? 0;
        }

        /// <summary>
        /// Check if the current user has liked a post
        /// </summary>
        public async Task<bool> HasUserLikedAsync(Guid postId)
        {
            var result = await _http.GetFromJsonAsync<LikeStatusResponse>($"api/likes/{postId}/status");
            return result?.Liked ?? false;
        }
    }

    // Match backend responses
    public class LikeCountResponse
    {
        public int Count { get; set; }
    }

    public class LikeStatusResponse
    {
        public bool Liked { get; set; }
    }
}
