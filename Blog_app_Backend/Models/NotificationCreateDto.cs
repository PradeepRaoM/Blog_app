using System;

namespace Blog_app_backend.Models
{
    // DTO for creating a new notification
    public class NotificationCreateDto
    {

        public string Type { get; set; }
        public string Content { get; set; }
        public Guid TargetUserId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string ReferenceType { get; set; }
    }
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public Guid TargetUserId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
