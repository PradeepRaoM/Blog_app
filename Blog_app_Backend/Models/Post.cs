using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blog_app_backend.Models
{
    [Table("posts")]
    public class Post : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("title")]
        public string Title { get; set; }

        [Required]
        [Column("content_markdown")]
        public string ContentMarkdown { get; set; }

        [Column("content_html")]
        public string ContentHtml { get; set; }

        [Column("author_id")]
        public Guid AuthorId { get; set; }

        [Column("user_id")]
        public Guid? UserId { get; set; }

        [Column("category_id")]
        public Guid? CategoryId { get; set; }

        [Column("status")]
        public string Status { get; set; } = "draft";

        [Column("featured_image_url")]
        [MaxLength(500)]
        public string FeaturedImageUrl { get; set; }

        [Column("featured_image_path")]
        public string FeaturedImagePath { get; set; }

        [Column("featured_image_mime")]
        public string FeaturedImageMime { get; set; }

        [Column("is_published")]
        public bool IsPublished { get; set; } = false;

        [Column("published_at")]
        public DateTime? PublishedAt { get; set; }

        [Column("scheduled_for")]
        public DateTime? ScheduledFor { get; set; }

        [Column("meta_title")]
        [MaxLength(60)]
        public string MetaTitle { get; set; }

        [Column("meta_description")]
        [MaxLength(160)]
        public string MetaDescription { get; set; }

        [Column("slug")]
        public string Slug { get; set; }

        [Column("tag_ids")]
        public List<Guid> TagIds { get; set; } = new List<Guid>();

        [Column("hashtags")]
        public List<string> Hashtags { get; set; } = new List<string>();

        [Column("location_tag")]
        public string LocationTag { get; set; }

        [Column("mentioned_user_ids")]
        public List<Guid> MentionedUserIds { get; set; } = new List<Guid>();

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
