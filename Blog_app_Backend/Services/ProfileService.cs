using Blog_app_backend.Models;
using Blog_app_backend.Supabase;
using Microsoft.AspNetCore.Http;
using Supabase;
using Supabase.Storage;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SupabaseClient = Supabase.Client;
using SupabaseFileOptions = Supabase.Storage.FileOptions;

namespace Blog_app_backend.Services
{
    public class ProfileService
    {
        private readonly SupabaseClient _supabase;
        private const string AvatarBucket = "avatars";

        public ProfileService(SupabaseClientProvider clientProvider)
        {
            _supabase = clientProvider.GetClient();
        }

        public async Task<Profile?> GetMyProfileAsync(Guid userId)
        {
            try
            {
                return await _supabase.From<Profile>()
                                      .Where(p => p.Id == userId)
                                      .Single();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetMyProfileAsync] Error: {ex.Message}");
                return null;
            }
        }

        public async Task<Profile?> CreateMyProfileAsync(Profile profile)
        {
            try
            {
                profile.CreatedAt = DateTime.UtcNow;
                profile.UpdatedAt = DateTime.UtcNow;

                var inserted = await _supabase.From<Profile>().Insert(profile);
                return inserted.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateMyProfileAsync] Error: {ex.Message}");
                return null;
            }
        }

        public async Task<Profile?> UpdateMyProfileAsync(Guid userId, Profile updatedProfile)
        {
            try
            {
                updatedProfile.Id = userId;
                updatedProfile.UpdatedAt = DateTime.UtcNow;

                var updated = await _supabase.From<Profile>().Update(updatedProfile);
                return updated.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateMyProfileAsync] Error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteMyProfileAsync(Guid userId)
        {
            try
            {
                await _supabase.From<Profile>().Where(p => p.Id == userId).Delete();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteMyProfileAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> UploadAvatarAsync(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            if (file.Length > 5_000_000)
                throw new ArgumentException("File exceeds 5MB limit");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType))
                throw new ArgumentException("Invalid file type. Only JPEG, PNG, GIF allowed");

            // Ensure profile exists
            var profile = await GetMyProfileAsync(userId);
            if (profile == null)
            {
                profile = new Profile
                {
                    Id = userId,
                    FullName = "Default User",
                    Username = $"user_{userId.ToString().Substring(0, 8)}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                profile = await CreateMyProfileAsync(profile);
            }

            if (profile == null)
                throw new Exception("Unable to create or fetch profile.");

            // Generate unique file name
            var fileName = Path.GetFileNameWithoutExtension(file.FileName) +
                           "_" + Guid.NewGuid() +
                           Path.GetExtension(file.FileName);

            var filePath = $"{userId}/{fileName}";
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            var bucket = _supabase.Storage.From(AvatarBucket);

            // Upload file
            await bucket.Upload(fileBytes, filePath, new SupabaseFileOptions { Upsert = true });

            var publicUrl = bucket.GetPublicUrl(filePath);

            // Update profile with avatar URL
            profile.AvatarUrl = publicUrl;
            await UpdateMyProfileAsync(userId, profile);

            return publicUrl;
        }

        public async Task<bool> RemoveAvatarAsync(Guid userId, string fileName)
        {
            try
            {
                var bucket = _supabase.Storage.From(AvatarBucket);
                await bucket.Remove($"{userId}/{fileName}");

                var profile = await GetMyProfileAsync(userId);
                if (profile != null && profile.AvatarUrl?.Contains(fileName) == true)
                {
                    profile.AvatarUrl = null;
                    await UpdateMyProfileAsync(userId, profile);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RemoveAvatarAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<Profile?> GetProfileByIdAsync(Guid userId)
        {
            try
            {
                return await _supabase.From<Profile>()
                                      .Where(p => p.Id == userId)
                                      .Single();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetProfileByIdAsync] Error: {ex.Message}");
                return null;
            }
        }
    }
}
