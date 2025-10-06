using Blog_app_backend.Models;
using Blog_app_backend.Supabase;
using Blog_app_Backend.Models;
using Markdig;
using Supabase;

using static Supabase.Postgrest.Constants;




namespace Blog_app_backend.Services
{
    public class PostService
    {
        private readonly Client _client;
        private readonly PostTagService _postTagService;
        private readonly LikeService _likeService;
        private readonly NotificationService _notificationService;

        public PostService(SupabaseClientProvider clientProvider, PostTagService postTagService, LikeService likeService, NotificationService notificationService)
        {
            _client = clientProvider.GetClient();
            _postTagService = postTagService;
            _likeService = likeService;
            _notificationService = notificationService;
        }

        // ✅ Create or update post
        public async Task<(PostDto Post, string Error)> CreateOrUpdatePostAsync(Post post, string userId)
        {
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
                return (null, "Invalid user ID.");

            post.UserId = userGuid;
            if (post.Id == Guid.Empty) post.Id = Guid.NewGuid();

            post.CreatedAt = post.CreatedAt == default ? DateTime.UtcNow : post.CreatedAt;
            post.UpdatedAt = DateTime.UtcNow;

            // Markdown → HTML
            if (!string.IsNullOrEmpty(post.ContentMarkdown))
            {
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                post.ContentHtml = Markdown.ToHtml(post.ContentMarkdown, pipeline);
            }

            // Slug + SEO
            post.Slug = string.IsNullOrWhiteSpace(post.Slug) ? GenerateSlug(post.Title) : post.Slug;
            post.MetaTitle = string.IsNullOrWhiteSpace(post.MetaTitle)
                ? (string.IsNullOrWhiteSpace(post.Title) ? "Untitled Post" : post.Title)
                : post.MetaTitle;
            post.MetaDescription = string.IsNullOrWhiteSpace(post.MetaDescription)
                ? GenerateMetaDescription(post.ContentMarkdown)
                : post.MetaDescription;

            // Publish / Schedule logic
            if (post.ScheduledFor != null && post.ScheduledFor > DateTime.UtcNow)
            {
                post.IsPublished = false;
                post.PublishedAt = null;
                post.Status = "scheduled";
            }
            else
            {
                post.IsPublished = post.IsPublished || post.Status == "published";
                post.PublishedAt = post.IsPublished ? (post.PublishedAt ?? DateTime.UtcNow) : null;
                post.Status = post.IsPublished ? "published" : post.Status ?? "draft";
            }

            post.Hashtags ??= new List<string>();
            post.MentionedUserIds ??= new List<Guid>();
            post.LocationTag ??= "";

            try
            {
                var result = await _client.From<Post>().Upsert(post);
                var savedPost = result.Models.FirstOrDefault();
                if (savedPost == null)
                    return (null, "Failed to save post.");

                var postDto = await BuildDto(savedPost, userGuid);

                // 🔹 Trigger notifications for mentioned users
                if (post.MentionedUserIds != null && post.MentionedUserIds.Any())
                {
                    foreach (var mentionedUserId in post.MentionedUserIds)
                    {
                        if (mentionedUserId == userGuid) continue; // skip author

                        var notification = new Notification
                        {
                            Type = "mention",
                            Content = $"You were mentioned in a post by {postDto.AuthorUsername}",
                            TargetUserId = mentionedUserId,
                            ReferenceId = savedPost.Id,
                            ReferenceType = "post"
                        };

                        try
                        {
                            await _notificationService.CreateNotificationAsync(notification);
                        }
                        catch
                        {
                            // ignore notification errors to not break post creation
                        }
                    }
                }

                return (postDto, null);
            }
            catch (Exception ex)
            {
                return (null, $"Database error: {ex.Message}");
            }
        }

        // ✅ Get by Id
        public async Task<PostDto> GetPostByIdAsync(Guid postId, Guid? currentUserId = null)
        {
            var result = await _client.From<Post>()
                .Filter("id", Operator.Equals, postId.ToString())
                .Get();

            var post = result.Models.FirstOrDefault();
            return post == null ? null : await BuildDto(post, currentUserId);
        }

        // ✅ Delete post
        public async Task<bool> DeletePostAsync(Guid postId, string userId)
        {
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
                return false;

            var result = await _client.From<Post>()
                .Filter("id", Operator.Equals, postId.ToString())
                .Filter("user_id", Operator.Equals, userGuid.ToString())
                .Get();

            var post = result.Models.FirstOrDefault();
            if (post == null) return false;

            await _postTagService.AssignTagsToPost(postId, new List<Guid>());
            await _client.From<Post>().Filter("id", Operator.Equals, postId.ToString()).Delete();

            return true;
        }

        // ✅ All posts by user (public profile)
        public async Task<List<PostDto>> GetAllPostsByUserId(string userId, Guid? currentUserId = null)
        {
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
                return new List<PostDto>();

            var now = DateTime.UtcNow;

            var result = await _client.From<Post>()
                .Filter("user_id", Operator.Equals, userGuid.ToString())
                .Filter("is_published", Operator.Equals, "true")
                .Filter("published_at", Operator.LessThanOrEqual, now.ToString("o"))
                .Order(p => p.PublishedAt, Ordering.Descending)
                .Get();

            return await BuildDtoList(result.Models, currentUserId);
        }

        // ✅ All published posts
        public async Task<List<PostDto>> GetAllPublishedPostsAsync(Guid? currentUserId = null)
        {
            var now = DateTime.UtcNow;

            var result = await _client.From<Post>()
                .Filter("is_published", Operator.Equals, "true")
                .Filter("published_at", Operator.LessThanOrEqual, now.ToString("o"))
                .Order(p => p.PublishedAt, Ordering.Descending)
                .Get();

            return await BuildDtoList(result.Models, currentUserId);
        }

        // ✅ By category
        public async Task<List<PostDto>> GetPostsByCategoryIdAsync(Guid categoryId, Guid? currentUserId = null)
        {
            var result = await _client.From<Post>()
                .Filter("category_id", Operator.Equals, categoryId.ToString())
                .Filter("is_published", Operator.Equals, "true")
                .Order(p => p.PublishedAt, Ordering.Descending)
                .Get();

            return await BuildDtoList(result.Models, currentUserId);
        }

        // ✅ By tag
        public async Task<List<PostDto>> GetPostsByTagIdAsync(Guid tagId, Guid? currentUserId = null)
        {
            var postTags = await _postTagService.GetPostsByTagId(tagId);
            if (postTags == null || !postTags.Any()) return new List<PostDto>();

            var postIds = postTags.Select(pt => pt.PostId.ToString()).ToList();

            var result = await _client.From<Post>()
                .Filter("is_published", Operator.Equals, "true")
                .Filter("id", Operator.In, $"({string.Join(",", postIds)})")
                .Order(p => p.PublishedAt, Ordering.Descending)
                .Get();

            return await BuildDtoList(result.Models, currentUserId);
        }

        // ✅ Feed (paged)
        public async Task<List<PostDto>> GetFeedAsync(int page = 1, int limit = 10, Guid? currentUserId = null)
        {
            page = Math.Max(page, 1);
            var skip = (page - 1) * limit;

            var now = DateTime.UtcNow;

            var result = await _client.From<Post>()
                .Filter("is_published", Operator.Equals, "true")
                .Filter("published_at", Operator.LessThanOrEqual, now.ToString("o"))
                .Order(p => p.PublishedAt, Ordering.Descending)
                .Range(skip, skip + limit - 1)
                .Get();

            return await BuildDtoList(result.Models, currentUserId);
        }

        // ✅ Search
        public async Task<List<PostDto>> SearchPostsAsync(string query, int page = 1, int limit = 10, Guid? currentUserId = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<PostDto>();

            page = Math.Max(page, 1);
            var skip = (page - 1) * limit;

            var now = DateTime.UtcNow;

            var result = await _client.From<Post>()
                .Filter("is_published", Operator.Equals, "true")
                .Filter("published_at", Operator.LessThanOrEqual, now.ToString("o"))
                .Filter("title", Operator.ILike, $"%{query}%")
                .Order(p => p.PublishedAt, Ordering.Descending)
                .Range(skip, skip + limit - 1)
                .Get();

            return await BuildDtoList(result.Models, currentUserId);
        }

        // ✅ Filter by date range
        public async Task<List<PostDto>> GetPostsByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? currentUserId = null)
        {
            var now = DateTime.UtcNow;

            var result = await _client.From<Post>()
                .Filter("is_published", Operator.Equals, "true")
                .Filter("published_at", Operator.GreaterThanOrEqual, startDate.ToString("o"))
                .Filter("published_at", Operator.LessThanOrEqual, endDate.ToString("o"))
                .Filter("published_at", Operator.LessThanOrEqual, now.ToString("o"))
                .Order(p => p.PublishedAt, Ordering.Descending)
                .Get();

            return await BuildDtoList(result.Models, currentUserId);
        }

        // --- Helpers ---
        private async Task<PostDto> BuildDto(Post post, Guid? currentUserId = null)
        {
            var dto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                ContentMarkdown = post.ContentMarkdown,
                ContentHtml = post.ContentHtml,
                Status = post.Status,
                FeaturedImageUrl = post.FeaturedImageUrl,
                IsPublished = post.IsPublished,
                PublishedAt = post.PublishedAt,
                ScheduledFor = post.ScheduledFor,
                MetaTitle = post.MetaTitle,
                MetaDescription = post.MetaDescription,
                Slug = post.Slug,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                CategoryId = post.CategoryId,
                Hashtags = post.Hashtags ?? new List<string>(),
                LocationTag = post.LocationTag,
                MentionedUserIds = post.MentionedUserIds ?? new List<Guid>(),
                TagIds = new List<Guid>()
            };

            // Author info
            if (post.UserId.HasValue)
            {
                try
                {
                    var profileResult = await _client.From<Profile>()
                        .Filter("id", Operator.Equals, post.UserId.Value.ToString())
                        .Get();

                    var profile = profileResult.Models.FirstOrDefault();
                    if (profile != null)
                    {
                        dto.AuthorId = profile.Id;
                        dto.AuthorFullName = profile.FullName ?? "Unknown";
                        dto.AuthorUsername = profile.Username ?? "unknown";
                        dto.AuthorAvatarUrl = profile.AvatarUrl;
                    }
                    else
                    {
                        dto.AuthorFullName = "Unknown";
                        dto.AuthorUsername = "unknown";
                    }
                }
                catch
                {
                    dto.AuthorFullName = "Unknown";
                    dto.AuthorUsername = "unknown";
                }
            }

            var tags = await _postTagService.GetTagsByPostId(post.Id);
            if (tags != null && tags.Any())
                dto.TagIds = tags.Select(t => t.Id).ToList();

            // ✅ Populate likes
            dto.LikeCount = await _likeService.GetLikesCountAsync(post.Id);
            if (currentUserId != null)
                dto.IsLikedByCurrentUser = await _likeService.HasUserLikedAsync(post.Id, currentUserId.Value);

            return dto;
        }

        private async Task<List<PostDto>> BuildDtoList(List<Post> posts, Guid? currentUserId = null)
        {
            var list = new List<PostDto>();
            foreach (var post in posts)
            {
                list.Add(await BuildDto(post, currentUserId));
            }
            return list;
        }

        // ✅ Get my posts (all statuses)
        public async Task<Dictionary<string, List<PostDto>>> GetMyPostsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid userGuid))
                return new Dictionary<string, List<PostDto>>();

            var result = await _client.From<Post>()
                .Filter("user_id", Operator.Equals, userGuid.ToString())
                .Order(p => p.CreatedAt, Ordering.Descending)
                .Get();

            var dtoList = await BuildDtoList(result.Models, userGuid);

            return dtoList
                .GroupBy(p => p.Status ?? "draft")
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        // get the archive post
        public async Task<Dictionary<string, List<PostDto>>> GetArchiveAsync()
        {
            var now = DateTime.UtcNow;

            var result = await _client.From<Post>()
                .Filter("is_published", Operator.Equals, "true")
                .Filter("published_at", Operator.LessThanOrEqual, now.ToString("o"))
                .Order(p => p.PublishedAt, Ordering.Descending)
                .Get();

            var posts = await BuildDtoList(result.Models);

            // Group by Year-Month
            var grouped = posts
                .Where(p => p.PublishedAt != null)
                .GroupBy(p => p.PublishedAt.Value.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.ToList());

            return grouped;
        }

        // ✅ Filter posts
        public async Task<List<PostDto>> FilterPostsAsync(PostFilter filter, Guid? currentUserId = null)
        {
            var query = _client.From<Post>()
                .Filter("is_published", Operator.Equals, "true")
                .Order(p => p.PublishedAt, Ordering.Descending);

            // Category filter
            if (filter.CategoryId.HasValue)
                query = query.Filter("category_id", Operator.Equals, filter.CategoryId.Value.ToString());

            // Author filter
            if (filter.AuthorId.HasValue)
                query = query.Filter("user_id", Operator.Equals, filter.AuthorId.Value.ToString());

            // Location filter
            if (!string.IsNullOrWhiteSpace(filter.LocationTag))
                query = query.Filter("location_tag", Operator.ILike, $"%{filter.LocationTag}%");

            // Hashtags filter
            if (filter.Hashtags != null && filter.Hashtags.Any())
            {
                foreach (var tag in filter.Hashtags)
                {
                    query = query.Filter("hashtags", Operator.ILike, $"%{tag}%");
                }
            }

            // Date range filter
            if (filter.StartDate.HasValue)
                query = query.Filter("published_at", Operator.GreaterThanOrEqual, filter.StartDate.Value.ToString("o"));
            if (filter.EndDate.HasValue)
                query = query.Filter("published_at", Operator.LessThanOrEqual, filter.EndDate.Value.ToString("o"));

            // Fetch posts
            var result = await query.Get();

            var posts = result.Models;

            // TagIds filter (many-to-many via PostTagService)
            if (filter.TagIds != null && filter.TagIds.Any())
            {
                var filteredPosts = new List<Post>();
                foreach (var post in posts)
                {
                    var postTags = await _postTagService.GetTagsByPostId(post.Id);
                    if (postTags.Any(t => filter.TagIds.Contains(t.Id)))
                    {
                        filteredPosts.Add(post);
                    }
                }
                posts = filteredPosts;
            }

            return await BuildDtoList(posts, currentUserId);
        }

        // related post 
        public async Task<List<PostDto>> GetRelatedPostsAsync(Guid postId, Guid? currentUserId = null)
        {
            var originalPostResult = await _client.From<Post>()
                .Filter("id", Operator.Equals, postId.ToString())
                .Get();

            var originalPost = originalPostResult.Models.FirstOrDefault();
            if (originalPost == null)
                return new List<PostDto>();

            var now = DateTime.UtcNow;

            // Get posts excluding the original post
            var result = await _client.From<Post>()
                .Filter("id", Operator.Not, postId.ToString()) // Correct usage of 'Not'
                .Filter("is_published", Operator.Equals, "true")
                .Filter("published_at", Operator.LessThanOrEqual, now.ToString("o"))
                .Order(p => p.PublishedAt, Ordering.Descending)
                .Get();

            var relatedPosts = result.Models
                .Where(p =>
                    p.CategoryId == originalPost.CategoryId ||
                    (p.Hashtags != null && originalPost.Hashtags != null && p.Hashtags.Intersect(originalPost.Hashtags).Any()))
                .Take(5)  // Limit related posts
                .ToList();

            return await BuildDtoList(relatedPosts, currentUserId);
        }

        public async Task<Profile> GetProfileByIdAsync(Guid profileId)
        {
            var result = await _client.From<Profile>()
                .Filter("id", Operator.Equals, profileId.ToString())
                .Get();

            return result.Models.FirstOrDefault();
        }

        // Filter by author username
        public async Task<List<PostDto>> GetPostsByAuthorAsync(string username, Guid? currentUserId = null)
        {
            var profileResult = await _client.From<Profile>()
                .Filter("username", Operator.Equals, username)
                .Get();

            var profile = profileResult.Models.FirstOrDefault();
            if (profile == null) return new List<PostDto>();

            return await GetAllPostsByUserId(profile.Id.ToString(), currentUserId);
        }

        // Filter by category name
        public async Task<List<PostDto>> GetPostsByCategoryNameAsync(string categoryName, Guid? currentUserId = null)
        {
            var categoryResult = await _client.From<Category>()
                .Filter("name", Operator.Equals, categoryName)
                .Get();

            var category = categoryResult.Models.FirstOrDefault();
            if (category == null) return new List<PostDto>();

            return await GetPostsByCategoryIdAsync(category.Id, currentUserId);
        }

        // Filter by tag name
        public async Task<List<PostDto>> GetPostsByTagNameAsync(string tagName, Guid? currentUserId = null)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return new List<PostDto>();

            // Case-insensitive search
            var tagResult = await _client.From<Tag>()
                .Filter("name", Operator.ILike, tagName.Trim())
                .Get();

            var tag = tagResult.Models.FirstOrDefault();
            if (tag == null)
                return new List<PostDto>(); // return empty list if no tag found

            // Fetch posts associated with this tag
            return await GetPostsByTagIdAsync(tag.Id, currentUserId);
        }

        // Filter by location
        public async Task<List<PostDto>> GetPostsByLocationAsync(string location, Guid? currentUserId = null)
        {
            var now = DateTime.UtcNow;

            var result = await _client.From<Post>()
                .Filter("is_published", Operator.Equals, "true")
                .Filter("published_at", Operator.LessThanOrEqual, now.ToString("o"))
                .Filter("location_tag", Operator.ILike, $"%{location}%")
                .Order(p => p.PublishedAt, Ordering.Descending)
                .Get();

            return await BuildDtoList(result.Models, currentUserId);
        }

        // ✅ Filter by date (YYYY-MM-DD)
        public async Task<List<PostDto>> GetPostsByDateAsync(DateTime date, Guid? currentUserId = null)
        {
            var start = date.Date;                // 00:00 of that day
            var end = date.Date.AddDays(1).AddTicks(-1); // 23:59:59.9999999

            var result = await _client.From<Post>()
                .Filter("is_published", Operator.Equals, "true")
                .Filter("published_at", Operator.GreaterThanOrEqual, start.ToString("o"))
                .Filter("published_at", Operator.LessThanOrEqual, end.ToString("o"))
                .Order(p => p.PublishedAt, Ordering.Descending)
                .Get();

            return await BuildDtoList(result.Models, currentUserId);
        }
        public async Task<PostInsightsDto> GetPostInsightsAsync(Guid postId)
        {
            var insights = new PostInsightsDto
            {
                PostId = postId
            };

            // --- Views ---
            var viewsResult = await _client.From<PostView>()
                .Filter("post_id", Operator.Equals, postId.ToString())
                .Order(v => v.CreatedAt, Ordering.Descending)
                .Get();

            insights.ViewsCount = viewsResult.Models.Count;

            foreach (var view in viewsResult.Models)
            {
                if (view.UserId.HasValue)
                {
                    var profile = await GetProfileByIdAsync(view.UserId.Value);
                    if (profile != null)
                    {
                        insights.ViewedByUsers.Add(new UserDto
                        {
                            UserId = (Guid)profile.Id,
                            Username = profile.Username ?? "unknown",
                            FullName = profile.FullName ?? "Unknown",
                            AvatarUrl = profile.AvatarUrl
                        });
                    }
                }
            }

            // --- Likes ---
            insights.LikesCount = await _likeService.GetLikesCountAsync(postId);

            // Make sure LikeService has this method:
            var likesUsers = await _likeService.GetUsersWhoLikedAsync(postId);
            foreach (var userId in likesUsers)
            {
                var profile = await GetProfileByIdAsync(userId);
                if (profile != null)
                {
                    insights.LikedByUsers.Add(new UserDto
                    {
                        UserId = (Guid)profile.Id,
                        Username = profile.Username ?? "unknown",
                        FullName = profile.FullName ?? "Unknown",
                        AvatarUrl = profile.AvatarUrl
                    });
                }
            }

            // --- Comments ---
            var commentsResult = await _client.From<Comment>()
                .Filter("post_id", Operator.Equals, postId.ToString())
                .Order(c => c.CreatedAt, Ordering.Ascending)
                .Get();

            insights.CommentsCount = commentsResult.Models.Count;

            foreach (var comment in commentsResult.Models)
            {
                var profile = await GetProfileByIdAsync(comment.AuthorId);
                insights.Comments.Add(new CommentWithUserDto
                {
                    CommentId = comment.Id,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    LikeCount = comment.LikeCount,
                    Author = profile != null ? new UserDto
                    {
                        UserId = (Guid)profile.Id,
                        Username = profile.Username ?? "unknown",
                        FullName = profile.FullName ?? "Unknown",
                        AvatarUrl = profile.AvatarUrl
                    } : new UserDto()
                });
            }

            // --- Saved Posts ---
            var savesResult = await _client.From<SavedPost>()
                .Filter("post_id", Operator.Equals, postId.ToString())
                .Get();

            insights.SavesCount = savesResult.Models.Count;

            foreach (var save in savesResult.Models)
            {
                if (save.UserId != Guid.Empty) // Already non-nullable in SavedPost
                {
                    var profile = await GetProfileByIdAsync(save.UserId);
                    if (profile != null)
                    {
                        insights.SavedByUsers.Add(new UserDto
                        {
                            UserId = (Guid)profile.Id,
                            Username = profile.Username ?? "unknown",
                            FullName = profile.FullName ?? "Unknown",
                            AvatarUrl = profile.AvatarUrl
                        });
                    }
                }
            }

            return insights;
        }
        public async Task AddPostViewAsync(Guid postId, Guid userId)
        {
            try
            {
                var existingViews = await _client
                    .From<PostView>()
                    .Where(pv => pv.PostId == postId && pv.UserId == userId)
                    .Get();

                if (!existingViews.Models.Any())
                {
                    await _client.From<PostView>().Insert(new PostView
                    {
                        Id = Guid.NewGuid(),
                        PostId = postId,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post view: {ex.Message}");
            }
        }








        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return Guid.NewGuid().ToString();

            var slug = title.ToLowerInvariant();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", " ").Trim();
            slug = slug.Replace(" ", "-");
            return slug;
        }

        private string GenerateMetaDescription(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return "";

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var html = Markdown.ToHtml(markdown, pipeline);
            var plainText = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty).Trim();
            return plainText.Length > 160 ? plainText.Substring(0, 157) + "..." : plainText;
        }
    }
}