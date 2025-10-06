using System;
using System.Collections.Generic;

namespace Blog_app_backend.Models
{
    public class PostInsightsDto
    {
        public Guid PostId { get; set; }

        // Views
        public int ViewsCount { get; set; }
        public List<UserDto> ViewedByUsers { get; set; } = new List<UserDto>();

        // Likes
        public int LikesCount { get; set; }
        public List<UserDto> LikedByUsers { get; set; } = new List<UserDto>();

        // Comments
        public int CommentsCount { get; set; }
        public List<CommentWithUserDto> Comments { get; set; } = new List<CommentWithUserDto>();

        // Saves
        public int SavesCount { get; set; }
        public List<UserDto> SavedByUsers { get; set; } = new List<UserDto>();
    }

    // Simple user info
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = "Unknown";
        public string FullName { get; set; } = "Unknown";
        public string AvatarUrl { get; set; }
    }

    // Comment + author info
    public class CommentWithUserDto
    {
        public Guid CommentId { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public UserDto Author { get; set; } = new UserDto();
        public int LikeCount { get; set; }
    }
}
