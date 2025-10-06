using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;

namespace Blog_app_Frontend.Models
{
    public class CreatePostDto
    {
        public string Title { get; set; }
        public string ContentMarkdown { get; set; }
        public string Status { get; set; } = "draft";
        public Guid? CategoryId { get; set; }
        public IBrowserFile FeaturedImageFile { get; set; } // For uploading new image
        public string FeaturedImageUrl { get; set; } // Optional, if updating only
        public List<Guid> TagIds { get; set; } = new List<Guid>();
        public List<string> Hashtags { get; set; } = new List<string>();
        public string LocationTag { get; set; }
        public List<Guid> MentionedUserIds { get; set; } = new List<Guid>();
        public DateTime? ScheduledFor { get; set; }
        
    }
}
