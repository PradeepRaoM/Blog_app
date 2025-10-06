namespace Blog_app_backend.Models
{
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

    public class ProfileResponseDto
    {
        public Guid Id { get; set; }
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
        public DateTime UpdatedAt { get; set; }
        public string Email { get; set; } // optional: fetch from Supabase Auth
    }
}
