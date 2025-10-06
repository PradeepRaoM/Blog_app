using System;

namespace Blog_app_backend.Models
{
    public class TagDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } // ✅ Add CreatedAt so controller can use it
    }
}
