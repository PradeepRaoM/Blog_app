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
    public class CommentService
    {
        private readonly Client _client;
        private readonly NotificationService _notificationService;

        public CommentService(SupabaseClientProvider clientProvider, NotificationService notificationService)
        {
            _client = clientProvider.GetClient();
            _notificationService = notificationService;
        }

        private CommentDto MapToDto(Comment c, Profile profile) => new CommentDto
        {
            Id = c.Id,
            PostId = c.PostId,
            AuthorId = c.AuthorId,
            Content = c.Content,
            LikeCount = c.LikeCount,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            AuthorFullName = profile?.FullName,
            AuthorUsername = profile?.Username,
            AuthorAvatarUrl = profile?.AvatarUrl
        };

        private async Task<Profile> GetAuthorProfileAsync(Guid authorId)
        {
            var result = await _client.From<Profile>()
                .Filter("id", Operator.Equals, authorId.ToString())
                .Get();

            return result.Models.FirstOrDefault();
        }

        private async Task<bool> DoesPostExistAsync(Guid postId)
        {
            var result = await _client.From<Post>()
                .Filter("id", Operator.Equals, postId.ToString())
                .Get();

            return result.Models.Any();
        }

        private async Task<string> GetAuthorName(Guid authorId)
        {
            var profile = await GetAuthorProfileAsync(authorId);
            return profile?.Username ?? "Someone";
        }

        public async Task<CommentDto> CreateCommentAsync(CommentCreateDto dto)
        {
            if (!await DoesPostExistAsync(dto.PostId))
                throw new Exception("Post does not exist.");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                PostId = dto.PostId,
                AuthorId = dto.AuthorId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _client.From<Comment>().Insert(comment);

            // Optional: wait briefly to ensure the insert is queryable
            await Task.Delay(50);

            var profile = await GetAuthorProfileAsync(dto.AuthorId);

            // 🔹 Notify post author (skip if author comments on own post)
            var postResult = await _client.From<Post>()
                .Filter("id", Operator.Equals, dto.PostId.ToString())
                .Get();
            var post = postResult.Models.FirstOrDefault();
            if (post != null && post.UserId != dto.AuthorId)
            {
                var notification = new Notification
                {
                    Type = "comment",
                    Content = $"New comment on your post by {await GetAuthorName(dto.AuthorId)}",
                    TargetUserId = post.UserId.Value,
                    ReferenceId = comment.Id,
                    ReferenceType = "comment"
                };
                await _notificationService.CreateNotificationAsync(notification);
            }

            // 🔹 Notify mentioned users in comment
            if (dto.MentionedUserIds != null && dto.MentionedUserIds.Any())
            {
                foreach (var mentionedUserId in dto.MentionedUserIds)
                {
                    if (mentionedUserId == dto.AuthorId) continue;

                    var notification = new Notification
                    {
                        Type = "mention",
                        Content = $"You were mentioned in a comment by {await GetAuthorName(dto.AuthorId)}",
                        TargetUserId = mentionedUserId,
                        ReferenceId = comment.Id,
                        ReferenceType = "comment"
                    };
                    await _notificationService.CreateNotificationAsync(notification);
                }
            }

            return MapToDto(comment, profile);
        }

        public async Task<CommentDto> UpdateCommentAsync(Guid commentId, Guid userId, string newContent)
        {
            var existingResult = await _client.From<Comment>()
                .Filter("id", Operator.Equals, commentId.ToString())
                .Filter("author_id", Operator.Equals, userId.ToString())
                .Get();

            var existing = existingResult.Models.FirstOrDefault();
            if (existing == null) return null;

            existing.Content = newContent;
            existing.UpdatedAt = DateTime.UtcNow;

            await _client.From<Comment>().Update(existing);

            var profile = await GetAuthorProfileAsync(existing.AuthorId);
            return MapToDto(existing, profile);
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var existingResult = await _client.From<Comment>()
                .Filter("id", Operator.Equals, commentId.ToString())
                .Filter("author_id", Operator.Equals, userId.ToString())
                .Get();

            var existing = existingResult.Models.FirstOrDefault();
            if (existing == null) return false;

            await _client.From<Comment>().Delete(existing);
            return true;
        }

        public async Task<List<CommentDto>> GetCommentsByPostIdAsync(Guid postId)
        {
            var commentsResult = await _client.From<Comment>()
                .Filter("post_id", Operator.Equals, postId.ToString())
                .Order(c => c.CreatedAt, Ordering.Ascending)
                .Get();

            var dtos = new List<CommentDto>();
            foreach (var c in commentsResult.Models)
            {
                var profile = await GetAuthorProfileAsync(c.AuthorId);
                dtos.Add(MapToDto(c, profile));
            }

            return dtos;
        }

        public async Task<CommentDto> LikeCommentAsync(Guid commentId)
        {
            var existingResult = await _client.From<Comment>()
                .Filter("id", Operator.Equals, commentId.ToString())
                .Get();

            var existing = existingResult.Models.FirstOrDefault();
            if (existing == null) return null;

            existing.LikeCount += 1;
            existing.UpdatedAt = DateTime.UtcNow;

            await _client.From<Comment>().Update(existing);

            var profile = await GetAuthorProfileAsync(existing.AuthorId);
            return MapToDto(existing, profile);
        }

        public async Task<CommentDto> DislikeCommentAsync(Guid commentId)
        {
            var existingResult = await _client.From<Comment>()
                .Filter("id", Operator.Equals, commentId.ToString())
                .Get();

            var existing = existingResult.Models.FirstOrDefault();
            if (existing == null) return null;

            existing.LikeCount = Math.Max(0, existing.LikeCount - 1);
            existing.UpdatedAt = DateTime.UtcNow;

            await _client.From<Comment>().Update(existing);

            var profile = await GetAuthorProfileAsync(existing.AuthorId);
            return MapToDto(existing, profile);
        }

        public async Task<Comment> GetCommentByIdAsync(Guid commentId)
        {
            var result = await _client.From<Comment>()
                .Filter("id", Operator.Equals, commentId.ToString())
                .Get();

            return result.Models.FirstOrDefault();
        }
    }
}
