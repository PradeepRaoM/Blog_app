using System;
using System.IO;
using System.Threading.Tasks;
using SupabaseClient = Supabase.Client;
using SupabaseStorage = Supabase.Storage;
using Blog_app_backend.Supabase;

namespace Blog_app_backend.Services
{
    public class ImageService
    {
        private readonly SupabaseClient _client;

        public ImageService(SupabaseClientProvider clientProvider)
        {
            _client = clientProvider.GetClient();
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string bucketName = "post-images")
        {
            var bucket = _client.Storage.From(bucketName);

            // Convert Stream to byte[]
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await fileStream.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            await bucket.Upload(fileBytes, fileName, new SupabaseStorage.FileOptions
            {
                CacheControl = "3600",
                Upsert = false
            });

            return bucket.GetPublicUrl(fileName);
        }

        public async Task DeleteImageAsync(string fileName, string bucketName = "post-images")
        {
            var bucket = _client.Storage.From(bucketName);
            // Remove expects a single string, not string[]
            await bucket.Remove(fileName);
        }
    }
}
