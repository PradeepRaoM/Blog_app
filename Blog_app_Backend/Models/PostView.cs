using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Blog_app_backend.Models
{
    [Table("post_views")]
    public class PostView : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("post_id")]
        public Guid PostId { get; set; }

        [Column("user_id")]
        public Guid? UserId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
