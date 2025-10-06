using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Text.Json.Serialization;

namespace Blog_app_backend.Models
{
    [Table("collections")]
    public class Collection : BaseModel
    {
        [PrimaryKey("id", false)]
        [JsonIgnore]  // Prevent serialization of this metadata property
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [Table("saved_posts")]
    public class SavedPost : BaseModel
    {
        [PrimaryKey("id", false)]
        [JsonIgnore]  // Prevent serialization of this metadata property
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("post_id")]
        public Guid PostId { get; set; }

        [Column("collection_id")]
        public Guid? CollectionId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
