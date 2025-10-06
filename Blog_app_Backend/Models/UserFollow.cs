using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Blog_app_backend.Models
{
    [Table("user_follows")]
    public class UserFollow : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid? Id { get; set; }

        [Column("follower_id")]
        public Guid FollowerId { get; set; }

        [Column("following_id")]
        public Guid FollowingId { get; set; }

        [Column("followed_at")]
        public DateTime FollowedAt { get; set; }
    }
}
