using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Blog_app_backend.Models
{
    [Table("notifications")]
    public class Notification : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("type")]
        public string Type { get; set; }             // e.g., 'comment', 'like', 'follow', 'mention'

        [Column("content")]
        public string Content { get; set; }          // Human-readable description

        [Column("target_user_id")]
        public Guid TargetUserId { get; set; }      // Profile Id of the target user

        [Column("reference_id")]
        public Guid? ReferenceId { get; set; }       // Related post, comment, or profile ID

        [Column("reference_type")]
        public string ReferenceType { get; set; }    // 'post', 'comment', 'profile'

        [Column("is_read")]
        public bool IsRead { get; set; } = false;    // Read/unread status

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
