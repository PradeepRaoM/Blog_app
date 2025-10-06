using Blog_app_backend.Models;
using Blog_app_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blog_app_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Utility: Get current user ID from JWT
        private Guid GetUserId()
        {
            var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out Guid userId))
                throw new UnauthorizedAccessException("Invalid user ID.");
            return userId;
        }

        // 🔹 CREATE NOTIFICATION
        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationCreateDto dto)
        {
            if (dto == null)
                return BadRequest("Notification data is required.");

            try
            {
                var notification = new Notification
                {
                    Type = dto.Type,
                    Content = dto.Content,
                    TargetUserId = dto.TargetUserId,
                    ReferenceId = dto.ReferenceId,
                    ReferenceType = dto.ReferenceType
                };

                var created = await _notificationService.CreateNotificationAsync(notification);

                // Map to DTO
                var resultDto = new NotificationDto
                {
                    Id = created.Id,
                    Type = created.Type,
                    Content = created.Content,
                    TargetUserId = created.TargetUserId,
                    ReferenceId = created.ReferenceId,
                    ReferenceType = created.ReferenceType,
                    IsRead = created.IsRead,
                    CreatedAt = created.CreatedAt
                };

                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create notification: {ex.Message}");
            }
        }


        // 🔹 GET UNREAD NOTIFICATIONS FOR CURRENT USER
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try
            {
                var userId = GetUserId();
                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);

                // Map to DTO
                var dtos = notifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Content = n.Content,
                    TargetUserId = n.TargetUserId,
                    ReferenceId = n.ReferenceId,
                    ReferenceType = n.ReferenceType,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to fetch notifications: {ex.Message}");
            }
        }


        // 🔹 GET NOTIFICATION BY ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetNotificationById(Guid id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null) return NotFound("Notification not found.");

                var dto = new NotificationDto
                {
                    Id = notification.Id,
                    Type = notification.Type,
                    Content = notification.Content,
                    TargetUserId = notification.TargetUserId,
                    ReferenceId = notification.ReferenceId,
                    ReferenceType = notification.ReferenceType,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to fetch notification: {ex.Message}");
            }
        }


        // 🔹 MARK AS READ
        [HttpPut("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            try
            {
                var success = await _notificationService.MarkAsReadAsync(id);
                if (!success) return NotFound("Notification not found.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to mark notification as read: {ex.Message}");
            }
        }

        // 🔹 DELETE NOTIFICATION
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            try
            {
                var success = await _notificationService.DeleteNotificationAsync(id);
                if (!success) return NotFound("Notification not found.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to delete notification: {ex.Message}");
            }
        }
    }
}
