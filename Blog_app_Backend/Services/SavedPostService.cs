using Blog_app_backend.Models;
using Blog_app_backend.Supabase;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Blog_app_backend.Services
{
    public class SavedPostService
    {
        private readonly Client _client;
        private readonly NotificationService _notificationService;

        public SavedPostService(SupabaseClientProvider clientProvider, NotificationService notificationService)
        {
            _client = clientProvider.GetClient();
            _notificationService = notificationService;
        }

        // Save a post or add it to a collection
        public async Task<SavedPostDto> SavePostAsync(Guid postId, Guid userId, Guid? collectionId = null)
        {
            try
            {
                string postIdStr = postId.ToString();
                string userIdStr = userId.ToString();

                // Validate post exists and is published
                var postResult = await _client.From<Post>()
                    .Filter("id", Operator.Equals, postIdStr)
                    .Filter("is_published", Operator.Equals, "true")
                    .Get();

                if (!postResult.Models.Any())
                    throw new Exception("Cannot save unpublished post or post not found.");

                // Validate collection if provided
                if (collectionId.HasValue)
                {
                    var collectionResult = await _client.From<Collection>()
                        .Filter("id", Operator.Equals, collectionId.Value.ToString())
                        .Filter("user_id", Operator.Equals, userIdStr)
                        .Get();

                    if (!collectionResult.Models.Any())
                        throw new Exception("Collection not found for this user.");
                }

                // Check for existing saved post
                var existingResult = await _client.From<SavedPost>()
                    .Filter("post_id", Operator.Equals, postIdStr)
                    .Filter("user_id", Operator.Equals, userIdStr)
                    .Get();

                if (existingResult.Models.Any())
                {
                    // Update collection if needed
                    var existingSavedPost = existingResult.Models.First();
                    if (collectionId.HasValue)
                    {
                        existingSavedPost.CollectionId = collectionId;
                        var updateResult = await _client.From<SavedPost>()
                            .Filter("id", Operator.Equals, existingSavedPost.Id.ToString())
                            .Update(existingSavedPost);

                        if (!updateResult.Models.Any())
                            throw new Exception("Failed to update saved post in database.");
                    }

                    return MapToDto(existingSavedPost);
                }

                // Insert new saved post
                var newSavedPost = new SavedPost
                {
                    Id = Guid.NewGuid(),
                    PostId = postId,
                    UserId = userId,
                    CollectionId = collectionId,
                    CreatedAt = DateTime.UtcNow
                };

                var insertResult = await _client.From<SavedPost>().Insert(newSavedPost);
                // 🔹 Trigger notification to post author (skip if user saves own post)
                if (postResult.Models.FirstOrDefault()?.UserId != userId)
                {
                    var post = postResult.Models.FirstOrDefault();
                    if (post != null && post.UserId.HasValue)
                    {
                        var notification = new Notification
                        {
                            Type = "save",
                            Content = $"{await GetUsernameAsync(userId)} saved your post",
                            TargetUserId = post.UserId.Value,
                            ReferenceId = post.Id,
                            ReferenceType = "post"
                        };

                        await _notificationService.CreateNotificationAsync(notification);
                    }
                }

                var savedPostModel = insertResult.Models.FirstOrDefault();
                if (savedPostModel == null)
                    throw new Exception("Failed to save post in database.");

                return MapToDto(savedPostModel);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in SavePostAsync: {ex.Message}", ex);
            }
        }

        public async Task<bool> RemoveSavedPostAsync(Guid postId, Guid userId)
        {
            try
            {
                await _client.From<SavedPost>()
                    .Filter("post_id", Operator.Equals, postId.ToString())
                    .Filter("user_id", Operator.Equals, userId.ToString())
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing saved post: {ex.Message}");
            }
        }

        public async Task<List<PostDto>> GetSavedPostsAsync(Guid userId, Guid? collectionId = null)
        {
            try
            {
                var query = _client.From<SavedPost>()
                    .Filter("user_id", Operator.Equals, userId.ToString());

                if (collectionId.HasValue)
                    query = query.Filter("collection_id", Operator.Equals, collectionId.Value.ToString());

                var savedPostsResult = await query.Order("created_at", Ordering.Descending).Get();
                var savedPosts = savedPostsResult.Models;

                var postDtos = new List<PostDto>();
                foreach (var sp in savedPosts)
                {
                    var postResult = await _client.From<Post>()
                        .Filter("id", Operator.Equals, sp.PostId.ToString())
                        .Filter("is_published", Operator.Equals, "true")
                        .Single();

                    if (postResult != null)
                    {
                        var profileResult = await _client.From<Profile>()
                            .Filter("id", Operator.Equals, postResult.AuthorId.ToString())
                            .Single();

                        postDtos.Add(new PostDto
                        {
                            Id = postResult.Id,
                            Title = postResult.Title,
                            ContentMarkdown = postResult.ContentMarkdown,
                            ContentHtml = postResult.ContentHtml,
                            FeaturedImageUrl = postResult.FeaturedImageUrl,
                            PublishedAt = postResult.PublishedAt,
                            AuthorId = postResult.AuthorId,
                            AuthorFullName = profileResult?.FullName,
                            AuthorUsername = profileResult?.Username,
                            AuthorAvatarUrl = profileResult?.AvatarUrl,
                            IsPublished = postResult.IsPublished,
                            CreatedAt = postResult.CreatedAt,
                            UpdatedAt = postResult.UpdatedAt,
                            CategoryId = postResult.CategoryId,
                            CollectionId = sp.CollectionId
                        });
                    }
                }

                return postDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching saved posts: {ex.Message}");
            }
        }

        public async Task<CollectionDto> CreateCollectionAsync(Guid userId, string name)
        {
            try
            {
                var collection = new Collection
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _client.From<Collection>().Insert(collection);
                var createdCollection = result.Models.FirstOrDefault();
                if (createdCollection == null)
                    throw new Exception("Failed to create collection in database.");

                return new CollectionDto
                {
                    Id = createdCollection.Id,
                    UserId = createdCollection.UserId,
                    Name = createdCollection.Name,
                    CreatedAt = createdCollection.CreatedAt,
                    UpdatedAt = createdCollection.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating collection: {ex.Message}");
            }
        }

        public async Task<List<CollectionDto>> GetCollectionsAsync(Guid userId)
        {
            try
            {
                var result = await _client.From<Collection>()
                    .Filter("user_id", Operator.Equals, userId.ToString())
                    .Order("created_at", Ordering.Descending)
                    .Get();

                return result.Models.Select(c => new CollectionDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching collections: {ex.Message}");
            }
        }

        public async Task<Collection> UpdateCollectionAsync(Guid collectionId, Guid userId, string newName)
        {
            try
            {
                var updateData = new Collection
                {
                    Name = newName,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _client.From<Collection>()
                    .Filter("id", Operator.Equals, collectionId.ToString())
                    .Filter("user_id", Operator.Equals, userId.ToString())
                    .Update(updateData);

                return result.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating collection: {ex.Message}");
            }
        }

        public async Task<bool> DeleteCollectionAsync(Guid collectionId, Guid userId)
        {
            try
            {
                // Delete the collection
                await _client.From<Collection>()
                    .Filter("id", Operator.Equals, collectionId.ToString())
                    .Filter("user_id", Operator.Equals, userId.ToString())
                    .Delete();

                // Remove collectionId from any saved posts
                var updateSavedPosts = new SavedPost { CollectionId = null };
                await _client.From<SavedPost>()
                    .Filter("collection_id", Operator.Equals, collectionId.ToString())
                    .Update(updateSavedPosts);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting collection: {ex.Message}");
            }
        }

        // Helper to map model -> DTO
        private SavedPostDto MapToDto(SavedPost sp)
        {
            return new SavedPostDto
            {
                Id = sp.Id,
                UserId = sp.UserId,
                PostId = sp.PostId,
                CollectionId = sp.CollectionId,
                CreatedAt = sp.CreatedAt
            };
        }
        private async Task<string> GetUsernameAsync(Guid userId)
        {
            var profileResult = await _client.From<Profile>()
                .Filter("id", Operator.Equals, userId.ToString())
                .Get();

            var profile = profileResult.Models.FirstOrDefault();
            return profile?.Username ?? "Someone";
        }

    }
}
