using Blog_app_Frontend.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Blog_app_Frontend.Services
{
    public class NotificationService
    {
        private readonly HttpClient _http;

        public NotificationService(HttpClient http)
        {
            _http = http;
        }

        // Get unread notifications for current user
        public async Task<List<NotificationDto>> GetUnreadNotificationsAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/Notification/unread");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<NotificationDto>>() ?? new List<NotificationDto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching unread notifications: {ex.Message}");
            }
        }

        // Mark notification as read
        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                var response = await _http.PutAsync($"api/Notification/{notificationId}/read", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error marking notification as read: {ex.Message}");
            }
        }

        // Delete a notification
        public async Task<bool> DeleteNotificationAsync(Guid notificationId)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Notification/{notificationId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting notification: {ex.Message}");
            }
        }

        // Optionally: create a notification (if frontend triggers it)
        public async Task<NotificationDto> CreateNotificationAsync(NotificationCreateDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Notification", dto);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<NotificationDto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating notification: {ex.Message}");
            }
        }
    }
}
