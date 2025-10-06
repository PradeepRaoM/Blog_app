using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Blog_app_Frontend.Services
{
    public class UserFollowService
    {
        private readonly HttpClient _http;
        private const string BaseEndpoint = "api/follow";

        public UserFollowService(HttpClient http)
        {
            _http = http;
        }

        // Follow a user
        public async Task<bool> FollowUserAsync(Guid userId)
        {
            var response = await _http.PostAsJsonAsync($"{BaseEndpoint}/{userId}", new { });
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            return true;
        }

        // Unfollow a user
        public async Task<bool> UnfollowUserAsync(Guid userId)
        {
            var response = await _http.PostAsJsonAsync($"{BaseEndpoint}/unfollow/{userId}", new { });
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(await response.Content.ReadAsStringAsync());
            return true;
        }

        // Get followers count
        public async Task<int> GetFollowersCountAsync(Guid userId)
        {
            var list = await _http.GetFromJsonAsync<List<Guid>>($"{BaseEndpoint}/followers/{userId}");
            return list?.Count ?? 0;
        }

        // Get following count
        public async Task<int> GetFollowingCountAsync(Guid userId)
        {
            var list = await _http.GetFromJsonAsync<List<Guid>>($"{BaseEndpoint}/following/{userId}");
            return list?.Count ?? 0;
        }

        // Check if current user is following another user
        public async Task<bool> IsFollowingAsync(Guid userId)
        {
            var result = await _http.GetFromJsonAsync<Dictionary<string, bool>>($"{BaseEndpoint}/isfollowing/{userId}");
            return result != null && result.TryGetValue("isFollowing", out var value) && value;
        }
    }
}
    