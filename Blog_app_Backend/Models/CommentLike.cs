using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Blog_app_backend.Models
{
    [Table("comment_likes")]
    public class CommentLike : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("comment_id")]
        public Guid CommentId { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("is_like")]
        public bool IsLike { get; set; }
    }
}
