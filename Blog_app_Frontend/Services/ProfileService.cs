using Blog_app_Frontend.Models;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Blog_app_Frontend.Services
{
    public class ProfileService
    {
        private readonly HttpClient _http;

        private const string BaseEndpoint = "api/Profile";
        private const string MeEndpoint = $"{BaseEndpoint}/me";
        private const string AvatarEndpoint = $"{MeEndpoint}/avatar";
        private const string FollowEndpoint = "api/UserFollow";

        public ProfileService(HttpClient http)
        {
            _http = http;
        }

        // ---------------- PROFILE + POSTS ----------------

        public async Task<ProfileWithPostsDto?> GetMyProfileWithPostsAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<ProfileWithPostsDto>(MeEndpoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetMyProfileWithPostsAsync failed: {ex.Message}");
                return null;
            }
        }

        public async Task<ProfileWithPostsDto?> GetProfileWithPostsAsync(Guid userId)
        {
            try
            {
                return await _http.GetFromJsonAsync<ProfileWithPostsDto>($"{BaseEndpoint}/{userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ GetProfileWithPostsAsync failed: {ex.Message}");
                return null;
            }
        }

        public async Task<Profile> CreateMyProfileAsync(ProfileCreateDto dto)
        {
            var response = await _http.PostAsJsonAsync(MeEndpoint, dto);

            if (!response.IsSuccessStatusCode)
                await HandleHttpError(response, "CreateMyProfileAsync");

            var wrapper = await response.Content.ReadFromJsonAsync<ProfileWithPostsDto>();
            return wrapper?.Profile ?? throw new Exception("Profile creation returned null.");
        }

        public async Task<Profile> UpdateMyProfileAsync(ProfileUpdateDto dto)
        {
            var response = await _http.PutAsJsonAsync(MeEndpoint, dto);

            if (!response.IsSuccessStatusCode)
                await HandleHttpError(response, "UpdateMyProfileAsync");

            var wrapper = await response.Content.ReadFromJsonAsync<ProfileWithPostsDto>();
            return wrapper?.Profile ?? throw new Exception("Profile update returned null.");
        }

        public async Task<bool> DeleteMyProfileAsync()
        {
            var response = await _http.DeleteAsync(MeEndpoint);

            if (!response.IsSuccessStatusCode)
                await HandleHttpError(response, "DeleteMyProfileAsync");

            return true;
        }

        // ---------------- AVATAR ----------------

        public async Task<string> UploadAvatarAsync(IBrowserFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType))
                throw new ArgumentException("❌ Invalid file type. Only JPEG, PNG, and GIF are allowed.");

            if (file.Size > 5_000_000) // 5 MB
                throw new ArgumentException("❌ File size exceeds 5MB limit");

            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream(5_000_000));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var response = await _http.PostAsync(AvatarEndpoint, content);

            if (!response.IsSuccessStatusCode)
                await HandleHttpError(response, "UploadAvatarAsync");

            var result = await response.Content.ReadFromJsonAsync<AvatarResponse>();
            return result?.AvatarUrl ?? throw new Exception("❌ Failed to retrieve avatar URL from server.");
        }

        public async Task<bool> RemoveAvatarAsync(string fileName)
        {
            var response = await _http.DeleteAsync($"{AvatarEndpoint}/{fileName}");

            if (!response.IsSuccessStatusCode)
                await HandleHttpError(response, "RemoveAvatarAsync");

            return true;
        }




        // ---------------- HELPERS ----------------

        private static async Task HandleHttpError(HttpResponseMessage response, string methodName)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ {methodName} failed ({response.StatusCode}): {errorContent}");
            throw new HttpRequestException($"Failed in {methodName}: {errorContent}", null, response.StatusCode);
        }

        private class AvatarResponse
        {
            public string AvatarUrl { get; set; } = string.Empty;
        }
    }

    // ---------------- DTO Wrapper ----------------
    public class ProfileWithPostsDto
    {
        public Profile Profile { get; set; } = new Profile();
        public List<PostDto> Posts { get; set; } = new();
    }
}
