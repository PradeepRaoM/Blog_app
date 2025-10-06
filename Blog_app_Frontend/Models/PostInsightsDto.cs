using System;
using System.Collections.Generic;

namespace Blog_app_Frontend.Models
{
    public class PostInsightsDto
    {
        public Guid PostId { get; set; }

        // Views
        public int ViewsCount { get; set; }
        public List<UserDto> ViewedByUsers { get; set; } = new();

        // Likes
        public int LikesCount { get; set; }
        public List<UserDto> LikedByUsers { get; set; } = new();

        // Comments
        public int CommentsCount { get; set; }
        public List<CommentWithUserDto> Comments { get; set; } = new();

        // Saves
        public int SavesCount { get; set; }
        public List<UserDto> SavedByUsers { get; set; } = new();
    }

    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = "unknown";
        public string FullName { get; set; } = "Unknown";
        public string AvatarUrl { get; set; } = "";
    }

    public class CommentWithUserDto
    {
        public Guid CommentId { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public UserDto Author { get; set; } = new();
    }
}
