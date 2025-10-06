using Supabase;
using Blog_app_backend.Models;
using Microsoft.Extensions.Options;

namespace Blog_app_backend.Supabase
{
    public class SupabaseClientProvider
    {
        private readonly Client _client;

        public SupabaseClientProvider(IOptions<SupabaseSettings> settings)
        {
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            _client = new Client(settings.Value.Url, settings.Value.AnonKey, options);
            _client.InitializeAsync().Wait();
        }

        public Client GetClient() => _client;
    }
}
