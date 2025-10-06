using System;

namespace Blog_app_Frontend.Models
{
    public class UserFollow
    {
        public Guid? Id { get; set; }
        public Guid FollowerId { get; set; }
        public Guid FollowingId { get; set; }
        public DateTime FollowedAt { get; set; }
    }
}
