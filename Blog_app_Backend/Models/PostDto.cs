using System;
using System.Collections.Generic;

namespace Blog_app_backend.Models
{
    public class PostDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string ContentMarkdown { get; set; }

        public string ContentHtml { get; set; }

        public string Status { get; set; }

        public string FeaturedImageUrl { get; set; }

        public bool IsPublished { get; set; }

        public DateTime? PublishedAt { get; set; }

        public DateTime? ScheduledFor { get; set; }

        public string MetaTitle { get; set; }

        public string MetaDescription { get; set; }

        public string Slug { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public Guid? CategoryId { get; set; }
        public CategoryDto Category { get; set; }

        public List<Guid> TagIds { get; set; } = new List<Guid>();
        public List<TagDto> Tags { get; set; } = new List<TagDto>();

        public List<string> Hashtags { get; set; } = new List<string>();

        public string LocationTag { get; set; }

        public List<Guid> MentionedUserIds { get; set; } = new List<Guid>();
        public Guid? AuthorId { get; set; }
        public string AuthorFullName { get; set; }
        public string AuthorUsername { get; set; }
        public string AuthorAvatarUrl { get; set; }
        public int LikeCount { get; set; } = 0;          // total likes
        public bool IsLikedByCurrentUser { get; set; } = false; // whether current user liked
        public Guid? CollectionId { get; set; }


    }
}
