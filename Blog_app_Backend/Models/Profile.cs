using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Blog_app_backend.Models
{
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid? Id { get; set; }

        [Column("full_name")]
        public string FullName { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("role")]
        public string Role { get; set; }

        [Column("avatar_url")]
        public string AvatarUrl { get; set; }

        [Column("bio")]
        public string Bio { get; set; }

        [Column("website")]
        public string Website { get; set; }

        [Column("twitter")]
        public string Twitter { get; set; }

        [Column("linkedin")]
        public string LinkedIn { get; set; }

        [Column("instagram")]
        public string Instagram { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
