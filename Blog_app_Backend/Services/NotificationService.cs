using Blog_app_backend.Models;
using Blog_app_backend.Supabase;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Blog_app_backend.Services
{
    public class NotificationService
    {
        private readonly Client _client;

        public NotificationService(SupabaseClientProvider clientProvider)
        {
            _client = clientProvider.GetClient();
        }

        // Create a notification (caller must ensure referenceId is valid)
        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            // Ensure default values
            notification.Id = notification.Id == Guid.Empty ? Guid.NewGuid() : notification.Id;
            notification.CreatedAt = notification.CreatedAt == default ? DateTime.UtcNow : notification.CreatedAt;
            notification.IsRead = notification.IsRead; // defaults to false in model

            var response = await _client.From<Notification>().Insert(notification);
            if (response.Models == null || !response.Models.Any())
                throw new Exception("Failed to create notification.");

            return response.Models.First();
        }

        // Get unread notifications for a user
        public async Task<List<Notification>> GetUnreadNotificationsAsync(Guid targetUserId)
        {
            var result = await _client.From<Notification>()
                .Filter("target_user_id", Operator.Equals, targetUserId.ToString())
                .Filter("is_read", Operator.Equals, "false")
                .Order(x => x.CreatedAt, Ordering.Descending)
                .Get();

            return result.Models ?? new List<Notification>();
        }

        public async Task<Notification> GetNotificationByIdAsync(Guid notificationId)
        {
            var result = await _client.From<Notification>()
                .Filter("id", Operator.Equals, notificationId.ToString())
                .Single();

            return result;
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            var notification = await GetNotificationByIdAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = true;
            var response = await _client.From<Notification>().Update(notification);
            return response.Models != null && response.Models.Any();
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId)
        {
            var notification = await GetNotificationByIdAsync(notificationId);
            if (notification == null)
                return false;

            await _client.From<Notification>().Delete(notification);
            return true;
        }
    }
}
