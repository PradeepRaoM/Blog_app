using System.Collections.Generic;

namespace Blog_app_Frontend.Models
{
    public class MyPostsResponse
    {
        public List<PostDto> Published { get; set; } = new();
        public List<PostDto> Draft { get; set; } = new();
        public List<PostDto> Scheduled { get; set; } = new();
    }
}
