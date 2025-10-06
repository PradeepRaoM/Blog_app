using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Blog_app_backend.Models
{
    [Table("tags")] // Make sure this matches your Supabase table name
    public class Tag : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
