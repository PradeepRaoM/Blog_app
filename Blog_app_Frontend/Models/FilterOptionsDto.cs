using System;
using System.Collections.Generic;

namespace Blog_app_Frontend.Models
{
    public class AuthorDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = "";
        public string Username { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
    }

    public class FilterOptionsDto
    {
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public List<TagDto> Tags { get; set; } = new List<TagDto>();
        public List<AuthorDto> Authors { get; set; } = new List<AuthorDto>();
        public List<string> Hashtags { get; set; } = new List<string>();
        public List<string> Locations { get; set; } = new List<string>();
    }
}
