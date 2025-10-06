public class CommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; }
    public int LikeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Author info
    public string AuthorFullName { get; set; }
    public string AuthorUsername { get; set; }
    public string AuthorAvatarUrl { get; set; }

    // New property
    public bool IsLikedByCurrentUser { get; set; }

}
