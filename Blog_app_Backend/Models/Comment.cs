using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Blog_app_backend.Models
{
    [Table("comments")]
    public class Comment : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("post_id")]
        public Guid PostId { get; set; }

        [Column("author_id")]
        public Guid AuthorId { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("like_count")]
        public int LikeCount { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
