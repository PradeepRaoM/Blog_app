namespace Blog_app_Frontend.Models
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid TargetUserId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationCreateDto
    {
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid TargetUserId { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
    }
}
