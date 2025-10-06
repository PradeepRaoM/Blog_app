using System;
using System.Collections.Generic;

namespace Blog_app_Frontend.Models
{
    public class Post
    {
        public Guid? Id { get; set; }

        public string Title { get; set; }

        public string ContentMarkdown { get; set; }

        public string ContentHtml { get; set; }

        public string Status { get; set; } = "draft"; // draft, published, scheduled

        public Guid? CategoryId { get; set; }

        public string FeaturedImageUrl { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedAt { get; set; }

        public DateTime? ScheduledFor { get; set; }

        public string MetaTitle { get; set; }

        public string MetaDescription { get; set; }

        public string Slug { get; set; }

        public List<Guid> TagIds { get; set; } = new List<Guid>();

        public List<string> Hashtags { get; set; } = new List<string>();

        public string LocationTag { get; set; }

        public List<Guid> MentionedUserIds { get; set; } = new List<Guid>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
