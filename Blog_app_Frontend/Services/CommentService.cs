using Blog_app_Frontend.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Blog_app_Frontend.Services
{
    // DTO for updating comment
    public class CommentUpdateRequest
    {
        public string Content { get; set; }
    }

    public class CommentService
    {
        private readonly HttpClient _http;

        public CommentService(HttpClient http)
        {
            _http = http;
        }

        // Create a comment
        public async Task<Comment> CreateCommentAsync(Guid postId, Guid authorId, string content)
        {
            var dto = new CommentCreateDto
            {
                PostId = postId,
                AuthorId = authorId,
                Content = content
            };

            var response = await _http.PostAsJsonAsync("api/comment", dto);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Comment>();
        }

        // Update a comment (only owner)
        public async Task<Comment> UpdateCommentAsync(Guid commentId, Guid userId, string newContent)
        {
            var request = new CommentUpdateRequest { Content = newContent };
            var response = await _http.PutAsJsonAsync($"api/comment/{commentId}/user/{userId}", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Comment>();
        }

        // Delete a comment (only owner)
        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var response = await _http.DeleteAsync($"api/comment/{commentId}/user/{userId}");
            return response.IsSuccessStatusCode;
        }

        // Get all comments for a post
        public async Task<List<Comment>> GetCommentsByPostIdAsync(Guid postId)
        {
            var response = await _http.GetAsync($"api/comment/post/{postId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Comment>>();
        }

        // Like a comment
        public async Task<Comment> LikeCommentAsync(Guid commentId)
        {
            var response = await _http.PostAsync($"api/comment/{commentId}/like", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Comment>();
        }

        // Dislike a comment
        public async Task<Comment> DislikeCommentAsync(Guid commentId)
        {
            var response = await _http.PostAsync($"api/comment/{commentId}/dislike", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Comment>();
        }
    }
}
