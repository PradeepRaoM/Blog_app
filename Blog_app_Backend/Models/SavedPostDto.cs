using System;

namespace Blog_app_backend.Models
{
    public class SavedPostDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public Guid? CollectionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class CollectionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
