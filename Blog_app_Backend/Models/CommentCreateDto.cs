using System;

namespace Blog_app_backend.Models
{
    public class CommentCreateDto
    {
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public string Content { get; set; }
        public List<Guid> MentionedUserIds { get; set; } = new List<Guid>();
    }
    public class CommentUpdateRequest
    {
        public string Content { get; set; }
    }
}
