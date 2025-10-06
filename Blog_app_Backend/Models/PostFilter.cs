namespace Blog_app_Backend.Models
{
    public class PostFilter
    {
        public Guid? CategoryId { get; set; }
        public List<Guid> TagIds { get; set; } = new();
        public Guid? AuthorId { get; set; }
        public string LocationTag { get; set; }
        public List<string> Hashtags { get; set; } = new();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
