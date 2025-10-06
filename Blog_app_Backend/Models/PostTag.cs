using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Blog_app_backend.Models
{
    [Table("post_tags")]
    public class PostTag : BaseModel
    {
        [Column("post_id")]
        public Guid PostId { get; set; }

        [Column("tag_id")]
        public Guid TagId { get; set; }
    }
}
