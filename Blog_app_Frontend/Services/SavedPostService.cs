using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blog_app_Frontend.Models;

namespace Blog_app_Frontend.Services
{
    public class SavedPostService
    {
        private readonly HttpClient _httpClient;

        public SavedPostService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SavedPostDto> SavePostAsync(Guid postId, Guid? collectionId = null)
        {
            try
            {
                var url = $"api/SavedPost/save/{postId}";
                if (collectionId.HasValue)
                    url += $"?collectionId={collectionId.Value}";

                var response = await _httpClient.PostAsJsonAsync(url, new { });
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    throw new HttpRequestException(error?.Message ?? "Failed to save post.");
                }

                return await response.Content.ReadFromJsonAsync<SavedPostDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving post: {ex.Message}");
            }
        }

        public async Task<bool> RemoveSavedPostAsync(Guid postId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/SavedPost/remove/{postId}");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    throw new HttpRequestException(error?.Message ?? "Failed to remove saved post.");
                }
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing saved post: {ex.Message}");
            }
        }

        public async Task<List<PostDto>> GetSavedPostsAsync(Guid? collectionId = null)
        {
            try
            {
                var url = "api/SavedPost";
                if (collectionId.HasValue)
                    url += $"?collectionId={collectionId.Value}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    throw new HttpRequestException(error?.Message ?? "Failed to fetch saved posts.");
                }

                return await response.Content.ReadFromJsonAsync<List<PostDto>>() ?? new List<PostDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching saved posts: {ex.Message}");
            }
        }

        public async Task<bool> IsPostSavedAsync(Guid postId)
        {
            try
            {
                var savedPosts = await GetSavedPostsAsync();
                return savedPosts.Exists(p => p.Id == postId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if post is saved: {ex.Message}");
            }
        }

        public async Task<CollectionDto> CreateCollectionAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Collection name cannot be empty.");

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/SavedPost/collection", new { Name = name });
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    throw new HttpRequestException(error?.Message ?? "Failed to create collection.");
                }

                return await response.Content.ReadFromJsonAsync<CollectionDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating collection: {ex.Message}");
            }
        }

        public async Task<List<CollectionDto>> GetCollectionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/SavedPost/collection");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    throw new HttpRequestException(error?.Message ?? "Failed to fetch collections.");
                }

                return await response.Content.ReadFromJsonAsync<List<CollectionDto>>() ?? new List<CollectionDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching collections: {ex.Message}");
            }
        }

        public async Task<CollectionDto> UpdateCollectionAsync(Guid collectionId, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("New collection name cannot be empty.");

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/SavedPost/collection/{collectionId}", new { Name = newName });
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    throw new HttpRequestException(error?.Message ?? "Failed to update collection.");
                }

                return await response.Content.ReadFromJsonAsync<CollectionDto>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating collection: {ex.Message}");
            }
        }

        public async Task<bool> DeleteCollectionAsync(Guid collectionId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/SavedPost/collection/{collectionId}");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    throw new HttpRequestException(error?.Message ?? "Failed to delete collection.");
                }
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting collection: {ex.Message}");
            }
        }

        public async Task<bool> SavePostToCollectionAsync(Guid postId, Guid collectionId)
        {
            try
            {
                var url = $"api/SavedPost/save/{postId}?collectionId={collectionId}";
                var response = await _httpClient.PostAsJsonAsync(url, new { });
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    throw new HttpRequestException(error?.Message ?? "Failed to add post to collection.");
                }
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding post to collection: {ex.Message}");
            }
        }

        private class ErrorResponse
        {
            public string Message { get; set; }
        }
    }
}