using System;
using System.Collections.Generic;

namespace Blog_app_Frontend.Models
{
    public class Profile
    {
        public Guid Id { get; set; }  // Always required

        public string FullName { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
        public string Website { get; set; }
        public string Twitter { get; set; }
        public string LinkedIn { get; set; }
        public string Instagram { get; set; }

        public DateTime CreatedAt { get; set; }

        // Nullable in case backend doesn’t always send this
        public DateTime? UpdatedAt { get; set; }

        public string Email { get; set; } // Optional, fetched from backend

        // Related posts authored by this user
        public List<PostDto> Posts { get; set; } = new List<PostDto>();

        // ✅ New properties for Follow system
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsFollowing { get; set; }
    }

    public class ProfileCreateDto
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
        public string Website { get; set; }
        public string Twitter { get; set; }
        public string LinkedIn { get; set; }
        public string Instagram { get; set; }
    }

    public class ProfileUpdateDto : ProfileCreateDto { }
}
