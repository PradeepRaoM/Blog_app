using System;

namespace Blog_app_Frontend.Models
{
    public class Comment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public string Content { get; set; }
        public int LikeCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Optional author info
        public string AuthorFullName { get; set; }
        public string AuthorUsername { get; set; }
        public string AuthorAvatarUrl { get; set; }

        // Frontend helper
        public bool IsLikedByCurrentUser { get; set; } = false;
    }

    public class CommentCreateDto
    {
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public string Content { get; set; }
    }
}
