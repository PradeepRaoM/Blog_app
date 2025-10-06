using Blog_app_Frontend.Models;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Blog_app_Frontend.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public AuthService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(email), "email" },
                { new StringContent(password), "password" }
            };

            var response = await _http.PostAsync("api/auth/login", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var session = await response.Content.ReadFromJsonAsync<SupabaseSession>();
            var token = session?.AccessToken;

            if (!string.IsNullOrEmpty(token))
            {
                await _js.InvokeVoidAsync("localStorage.setItem", "authToken", token);
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return token;
        }

        public async Task<bool> RegisterAsync(string email, string password)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(email), "email" },
                { new StringContent(password), "password" }
            };

            var response = await _http.PostAsync("api/auth/register", content);
            return response.IsSuccessStatusCode;
        }

        public async Task LogoutAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
            _http.DefaultRequestHeaders.Authorization = null;
        }

        public async Task LoadTokenAsync()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<Profile?> GetProfileAsync(Guid id)
        {
            var response = await _http.GetAsync($"api/profile/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Profile>();
        }

        public async Task<bool> UpsertProfileAsync(Profile profile)
        {
            var response = await _http.PostAsJsonAsync("api/profile", profile);
            return response.IsSuccessStatusCode;
        }

        public async Task<string?> GetTokenAsync()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
            return token;
        }

        public async Task<string?> GetTokenFromLocalStorageAsync()
        {
            return await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
        }
        public async Task<bool> ResetPasswordAsync(string email)
        {
            var content = new MultipartFormDataContent
    {
        { new StringContent(email), "email" }
    };

            var response = await _http.PostAsync("api/auth/reset-password", content);

            return response.IsSuccessStatusCode;
        }


    }

    public class SupabaseSession
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        // Add other Supabase session properties if needed
    }
}
