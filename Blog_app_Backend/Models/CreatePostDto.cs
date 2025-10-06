using System.ComponentModel.DataAnnotations;

public class CreatePostDto
{
    [Required] public string Title { get; set; }
    [Required] public string ContentMarkdown { get; set; }

    // Optional fields (backend auto-generates)
    public string Status { get; set; } = "draft";
    public DateTime? ScheduledFor { get; set; }
    public Guid? CategoryId { get; set; }
    public List<Guid> TagIds { get; set; } = new();
    public List<string> Hashtags { get; set; } = new();
    public string LocationTag { get; set; }
    public string MetaTitle { get; set; }
    public string MetaDescription { get; set; }
    public string Slug { get; set; }
    public List<Guid> MentionedUserIds { get; set; } = new();
}
