using Blog_app_backend.Supabase;
using Supabase.Gotrue;
using System.Threading.Tasks;
using SupabaseClient = Supabase.Client; 

namespace Blog_app_backend.Services
{
    public class AuthService
    {
        private readonly SupabaseClient _client;

        public AuthService(SupabaseClientProvider clientProvider)
        {
            _client = clientProvider.GetClient();
        }

        public async Task<Session> RegisterAsync(string email, string password)
        {
            var session = await _client.Auth.SignUp(email, password);
            return session;
        }

        public async Task<Session> LoginAsync(string email, string password)
        {
            var session = await _client.Auth.SignInWithPassword(email, password);
            return session;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            await _client.Auth.ResetPasswordForEmail(email);
            return true;
        }
    }
}
