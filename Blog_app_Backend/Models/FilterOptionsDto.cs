using System;
using System.Collections.Generic;

namespace Blog_app_backend.Models
{
    // 🔹 DTO for filter options response
    public class FilterOptionsDto
    {
        public List<CategoryDto> Categories { get; set; } = new();
        public List<TagDto> Tags { get; set; } = new();
        public List<AuthorDto> Authors { get; set; } = new();
        public List<string> Hashtags { get; set; } = new();
        public List<string> Locations { get; set; } = new();
    }

    // 🔹 Category DTO


    // 🔹 Author DTO
    public class AuthorDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
    }
}
