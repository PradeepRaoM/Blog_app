using Blog_app_backend.Models;
using Blog_app_backend.Services;
using Blog_app_Backend.Models;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blog_app_backend.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly PostTagService _postTagService;
        private readonly ImageService _imageService;
        private readonly CategoryService _categoryService;
        private readonly TagService _tagService;
        private readonly LikeService _likeService;

        public PostController(
            PostService postService,
            PostTagService postTagService,
            ImageService imageService,
            CategoryService categoryService,
            TagService tagService,
            LikeService likeService)
        {
            _postService = postService;
            _postTagService = postTagService;
            _imageService = imageService;
            _categoryService = categoryService;
            _tagService = tagService;
            _likeService = likeService;
        }

        // 🔹 Utility: get user ID from JWT claims
        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdStr, out Guid userId)) return userId;
            return null;
        }

        // 🔹 CREATE POST
        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto dto, IFormFile featuredImage)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("Invalid user.");

            try
            {
                string featuredImageUrl = null;
                if (featuredImage != null && featuredImage.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(featuredImage.FileName)}";
                    featuredImageUrl = await _imageService.UploadImageAsync(featuredImage.OpenReadStream(), fileName);
                }

                if (dto.CategoryId != null)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(dto.CategoryId.Value);
                    if (category == null) return BadRequest("Invalid category ID.");
                }

                if (dto.TagIds != null && dto.TagIds.Any())
                {
                    var allTags = await _tagService.GetAllTagsAsync();
                    var invalidTags = dto.TagIds.Except(allTags.Select(t => t.Id)).ToList();
                    if (invalidTags.Any())
                        return BadRequest($"Invalid tag IDs: {string.Join(", ", invalidTags)}");
                }

                dto.Slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Title) : dto.Slug;
                dto.MetaTitle = string.IsNullOrWhiteSpace(dto.MetaTitle) ? dto.Title ?? "Untitled Post" : dto.MetaTitle;
                dto.MetaDescription = string.IsNullOrWhiteSpace(dto.MetaDescription) ? GenerateMetaDescription(dto.ContentMarkdown) : dto.MetaDescription;

                string contentHtml = string.IsNullOrEmpty(dto.ContentMarkdown) ? "" : Markdown.ToHtml(dto.ContentMarkdown);

                var post = new Post
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    ContentMarkdown = dto.ContentMarkdown,
                    ContentHtml = contentHtml,
                    Status = dto.Status ?? "draft",
                    FeaturedImageUrl = featuredImageUrl,
                    ScheduledFor = dto.ScheduledFor,
                    CategoryId = dto.CategoryId,
                    MetaTitle = dto.MetaTitle,
                    MetaDescription = dto.MetaDescription,
                    Slug = dto.Slug,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserId = userId.Value,
                    Hashtags = dto.Hashtags ?? new List<string>(),
                    LocationTag = dto.LocationTag,
                    MentionedUserIds = dto.MentionedUserIds ?? new List<Guid>()
                };

                var (result, error) = await _postService.CreateOrUpdatePostAsync(post, userId.Value.ToString());
                if (result == null) return StatusCode(500, $"Failed to create post: {error}");

                if (dto.TagIds != null && dto.TagIds.Any())
                    await _postTagService.AssignTagsToPost(result.Id, dto.TagIds);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        // 🔹 UPDATE POST
        [HttpPut("{id}")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromForm] CreatePostDto dto, IFormFile featuredImage)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("Invalid user.");

            var existingPost = await _postService.GetPostByIdAsync(id, userId);
            if (existingPost == null) return NotFound("Post not found.");

            try
            {
                string featuredImageUrl = existingPost.FeaturedImageUrl;
                if (featuredImage != null && featuredImage.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(featuredImage.FileName)}";
                    featuredImageUrl = await _imageService.UploadImageAsync(featuredImage.OpenReadStream(), fileName);
                }

                if (dto.CategoryId != null)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(dto.CategoryId.Value);
                    if (category == null) return BadRequest("Invalid category ID.");
                }

                if (dto.TagIds != null && dto.TagIds.Any())
                {
                    var allTags = await _tagService.GetAllTagsAsync();
                    var invalidTags = dto.TagIds.Except(allTags.Select(t => t.Id)).ToList();
                    if (invalidTags.Any())
                        return BadRequest($"Invalid tag IDs: {string.Join(", ", invalidTags)}");
                }

                dto.Slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Title) : dto.Slug;
                dto.MetaTitle = string.IsNullOrWhiteSpace(dto.MetaTitle) ? dto.Title : dto.MetaTitle;
                dto.MetaDescription = string.IsNullOrWhiteSpace(dto.MetaDescription) ? GenerateMetaDescription(dto.ContentMarkdown) : dto.MetaDescription;

                string contentHtml = string.IsNullOrEmpty(dto.ContentMarkdown) ? existingPost.ContentHtml : Markdown.ToHtml(dto.ContentMarkdown);

                var postToUpdate = new Post
                {
                    Id = existingPost.Id,
                    Title = dto.Title,
                    ContentMarkdown = dto.ContentMarkdown,
                    ContentHtml = contentHtml,
                    Status = dto.Status ?? existingPost.Status,
                    FeaturedImageUrl = featuredImageUrl,
                    ScheduledFor = dto.ScheduledFor,
                    CategoryId = dto.CategoryId,
                    MetaTitle = dto.MetaTitle,
                    MetaDescription = dto.MetaDescription,
                    Slug = dto.Slug,
                    CreatedAt = existingPost.CreatedAt,
                    UpdatedAt = DateTime.UtcNow,
                    UserId = userId.Value,
                    Hashtags = dto.Hashtags ?? existingPost.Hashtags,
                    LocationTag = dto.LocationTag ?? existingPost.LocationTag,
                    MentionedUserIds = dto.MentionedUserIds ?? existingPost.MentionedUserIds
                };

                var (result, error) = await _postService.CreateOrUpdatePostAsync(postToUpdate, userId.Value.ToString());
                if (result == null) return StatusCode(500, $"Failed to update post: {error}");

                if (dto.TagIds != null && dto.TagIds.Any())
                    await _postTagService.AssignTagsToPost(result.Id, dto.TagIds);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        // 🔹 GET SINGLE POST
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPost(Guid id)
        {
            var currentUser = GetUserId();
            var post = await _postService.GetPostByIdAsync(id, currentUser);
            if (post == null) return NotFound(new { Message = "Post not found" });
            return Ok(post);
        }

        // 🔹 DELETE POST
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("Invalid user.");

            var success = await _postService.DeletePostAsync(id, userId.Value.ToString());
            if (!success) return NotFound(new { Message = "Post not found or unauthorized" });

            return NoContent();
        }

        // 🔹 GET MY POSTS (Authenticated user only)
        [HttpGet("my-posts")]
        [Authorize]
        public async Task<IActionResult> GetMyPosts()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized("Invalid user.");

            var posts = await _postService.GetMyPostsAsync(userId.Value.ToString());
            return Ok(posts);
        }

        // 🔹 GET POSTS BY ANY USER (for profile page)
        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublishedPostsByUser(Guid userId)
        {
            var currentUser = GetUserId();
            var posts = await _postService.GetAllPostsByUserId(userId.ToString(), currentUser);
            if (posts == null) return NotFound(new { Message = "No posts found for this user." });

            var publishedPosts = posts.Where(p => p.Status == "published").ToList();
            return Ok(publishedPosts);
        }

        // 🔹 GET PUBLISHED POSTS
        [HttpGet("published")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublishedPosts()
        {
            var currentUser = GetUserId();
            var posts = await _postService.GetAllPublishedPostsAsync(currentUser);
            return Ok(posts);
        }

        // 🔹 GET POSTS BY CATEGORY
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostsByCategory(Guid categoryId)
        {
            var currentUser = GetUserId();
            var posts = await _postService.GetPostsByCategoryIdAsync(categoryId, currentUser);
            return Ok(posts);
        }

        // 🔹 GET POSTS BY TAG
        [HttpGet("tag/{tagId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostsByTag(Guid tagId)
        {
            var currentUser = GetUserId();
            var posts = await _postService.GetPostsByTagIdAsync(tagId, currentUser);
            return Ok(posts);
        }

        // 🔹 GET FEED (with pagination)
        [HttpGet("feed")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var currentUser = GetUserId();
            var posts = await _postService.GetFeedAsync(page, limit, currentUser);
            return Ok(posts);
        }

        // 🔹 SEARCH POSTS
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchPosts([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("Search query is required.");

            var currentUser = GetUserId();
            var posts = await _postService.SearchPostsAsync(q, page, limit, currentUser);
            return Ok(posts);
        }
        // archive post
        [HttpGet("archive")]
        [AllowAnonymous]
        public async Task<IActionResult> GetArchive()
        {
            var archive = await _postService.GetArchiveAsync();
            return Ok(archive);
        }
        //related post
        [HttpGet("{postId}/related")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRelatedPosts(Guid postId)
        {
            var currentUser = GetUserId();
            var posts = await _postService.GetRelatedPostsAsync(postId, currentUser);
            return Ok(posts);
        }
        // 🔹 FILTER POSTS
        // 🔹 GET FILTER OPTIONS
        [HttpGet("filters")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFilterOptions()
        {
            try
            {
                // 1️⃣ Categories
                var categories = await _categoryService.GetAllCategoriesAsync();
                var categoryDtos = categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList();

                // 2️⃣ Tags
                var tags = await _tagService.GetAllTagsAsync();
                var tagDtos = tags.Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt
                }).ToList();

                // 3️⃣ Authors (profiles who have at least one post)
                var posts = await _postService.GetAllPublishedPostsAsync();
                var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();

                var authors = new List<AuthorDto>();

                foreach (var authorId in authorIds)
                {
                    if (!authorId.HasValue)
                        continue; // Skip if null

                    var profile = await _postService.GetProfileByIdAsync(authorId.Value); // Explicitly use .Value
                    if (profile != null)
                    {
                        authors.Add(new AuthorDto
                        {
                            Id = (Guid)profile.Id,
                            FullName = profile.FullName ?? "Unknown",
                            Username = profile.Username ?? "unknown",
                            AvatarUrl = profile.AvatarUrl // Make sure AuthorDto has AvatarUrl property
                        });
                    }
                }




                // 5️⃣ Locations (distinct)
                var locations = posts
                    .Where(p => !string.IsNullOrWhiteSpace(p.LocationTag))
                    .Select(p => p.LocationTag)
                    .Distinct()
                    .ToList();

                var filterOptions = new FilterOptionsDto
                {
                    Categories = categoryDtos,
                    Tags = tagDtos,
                    Authors = authors,

                    Locations = locations
                };

                return Ok(filterOptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }
        // 🔹 GET POSTS BY FILTER
        // 🔹 GET POSTS BY FILTER
        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostsByFilter(
            [FromQuery] string authors = null,
            [FromQuery] string categories = null,
            [FromQuery] string tags = null,
            [FromQuery] string locations = null,
            [FromQuery] string dates = null)
        {
            var currentUser = GetUserId();
            var allPosts = new List<PostDto>();

            try
            {
                List<List<PostDto>> filterResults = new();

                // 🔹 Authors filter
                if (!string.IsNullOrWhiteSpace(authors))
                {
                    var authorList = authors.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var postsByAuthors = new List<PostDto>();
                    foreach (var author in authorList)
                    {
                        var posts = await _postService.GetPostsByAuthorAsync(author, currentUser);
                        postsByAuthors.AddRange(posts);
                    }
                    filterResults.Add(postsByAuthors);
                }

                // 🔹 Categories filter
                if (!string.IsNullOrWhiteSpace(categories))
                {
                    var categoryList = categories.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var postsByCategories = new List<PostDto>();
                    foreach (var cat in categoryList)
                    {
                        var posts = await _postService.GetPostsByCategoryNameAsync(cat, currentUser);
                        postsByCategories.AddRange(posts);
                    }
                    filterResults.Add(postsByCategories);
                }

                // 🔹 Tags filter
                if (!string.IsNullOrWhiteSpace(tags))
                {
                    var tagList = tags.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var postsByTags = new List<PostDto>();
                    foreach (var tag in tagList)
                    {
                        var posts = await _postService.GetPostsByTagNameAsync(tag, currentUser);
                        postsByTags.AddRange(posts);
                    }
                    filterResults.Add(postsByTags);
                }

                // 🔹 Locations filter
                if (!string.IsNullOrWhiteSpace(locations))
                {
                    var locationList = locations.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var postsByLocations = new List<PostDto>();
                    foreach (var loc in locationList)
                    {
                        var posts = await _postService.GetPostsByLocationAsync(loc, currentUser);
                        postsByLocations.AddRange(posts);
                    }
                    filterResults.Add(postsByLocations);
                }

                // 🔹 Dates filter (YYYY-MM-DD)
                if (!string.IsNullOrWhiteSpace(dates))
                {
                    var dateList = dates.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var postsByDates = new List<PostDto>();
                    foreach (var dt in dateList)
                    {
                        if (DateTime.TryParse(dt, out DateTime parsedDate))
                        {
                            var posts = await _postService.GetPostsByDateAsync(parsedDate, currentUser);
                            postsByDates.AddRange(posts);
                        }
                    }
                    filterResults.Add(postsByDates);
                }

                // 🔹 Merge filters (intersect results if multiple filters applied)
                if (filterResults.Count == 0)
                {
                    // No filters, return all published posts
                    allPosts = await _postService.GetAllPublishedPostsAsync(currentUser);
                }
                else
                {
                    // Intersect all filtered results
                    allPosts = filterResults
                        .Select(list => list.Select(p => p.Id).ToHashSet())
                        .Aggregate((prev, next) => { prev.IntersectWith(next); return prev; })
                        .Select(id => filterResults.SelectMany(x => x).First(p => p.Id == id))
                        .ToList();
                }

                return Ok(allPosts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error while filtering posts: {ex.Message}");
            }
        }
        // --- Get Post Insights ---
        [HttpGet("{postId}/insights")]
        public async Task<IActionResult> GetPostInsights(Guid postId)
        {
            try
            {
                // --- Get current user ID from JWT claims ---
                Guid? currentUserId = null;
                if (User.Identity.IsAuthenticated)
                {
                    var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub"); // or "user_id"
                    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                        currentUserId = userId;
                }

                // ✅ Record the view if user is logged in
                if (currentUserId.HasValue)
                {
                    await _postService.AddPostViewAsync(postId, currentUserId.Value);
                }

                // Fetch post insights
                var insights = await _postService.GetPostInsightsAsync(postId);

                if (insights == null)
                    return NotFound(new { message = "Post not found or no insights available." });

                return Ok(insights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving post insights.", details = ex.Message });
            }
        }










        // 🔹 Utilities
        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title)) return Guid.NewGuid().ToString();
            var slug = title.ToLowerInvariant();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", " ").Trim();
            return slug.Replace(" ", "-");
        }

        private string GenerateMetaDescription(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return "";
            var html = Markdown.ToHtml(markdown);
            var plainText = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty).Trim();
            return plainText.Length > 160 ? plainText.Substring(0, 157) + "..." : plainText;
        }
    }
}
