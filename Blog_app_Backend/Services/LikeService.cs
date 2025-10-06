using Blog_app_backend.Models;
using Blog_app_backend.Supabase;
using Supabase;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Blog_app_backend.Services
{
    public class LikeService
    {
        private readonly Client _client;
        private readonly NotificationService _notificationService;

        public LikeService(SupabaseClientProvider clientProvider, NotificationService notificationService)
        {
            _client = clientProvider.GetClient();
            _notificationService = notificationService;
        }

        // Helper: Get post info directly from Supabase
        private async Task<Post> GetPostByIdAsync(Guid postId)
        {
            var result = await _client.From<Post>()
                .Filter("id", Operator.Equals, postId.ToString())
                .Single();

            return result;
        }

        // Helper: Get username by userId
        private async Task<string> GetUsernameAsync(Guid userId)
        {
            var profileResult = await _client.From<Profile>()
                .Filter("id", Operator.Equals, userId.ToString())
                .Single();

            return profileResult?.Username ?? "Someone";
        }

        // Like a post (trigger notification)
        public async Task<bool> LikeAsync(Guid postId, Guid userId)
        {
            var existing = await _client
                .From<PostLike>()
                .Filter("post_id", Operator.Equals, postId.ToString())
                .Filter("user_id", Operator.Equals, userId.ToString())
                .Get();

            if (existing.Models.Any()) return false; // already liked

            var newLike = new PostLike
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _client.From<PostLike>().Insert(newLike);

            // 🔹 Trigger notification to post author
            var post = await GetPostByIdAsync(postId);
            if (post != null && post.UserId != userId) // don't notify self
            {
                var notification = new Notification
                {
                    Type = "like",
                    Content = $"{await GetUsernameAsync(userId)} liked your post",
                    TargetUserId = post.UserId.Value,
                    ReferenceId = post.Id,
                    ReferenceType = "post"
                };

                // NotificationService no longer depends on PostService
                await _notificationService.CreateNotificationAsync(notification);
            }

            return true;
        }

        public async Task<bool> RemoveLikeAsync(Guid postId, Guid userId)
        {
            await _client
                .From<PostLike>()
                .Filter("post_id", Operator.Equals, postId.ToString())
                .Filter("user_id", Operator.Equals, userId.ToString())
                .Delete();

            return true;
        }

        public async Task<int> GetLikesCountAsync(Guid postId)
        {
            var result = await _client
                .From<PostLike>()
                .Filter("post_id", Operator.Equals, postId.ToString())
                .Get();

            return result?.Models?.Count ?? 0;
        }

        public async Task<bool> HasUserLikedAsync(Guid postId, Guid userId)
        {
            var existing = await _client
                .From<PostLike>()
                .Filter("post_id", Operator.Equals, postId.ToString())
                .Filter("user_id", Operator.Equals, userId.ToString())
                .Get();

            return existing?.Models?.Any() ?? false;
        }
        public async Task<List<Guid>> GetUsersWhoLikedAsync(Guid postId)
        {
            var result = await _client.From<PostLike>()
                .Filter("post_id", Operator.Equals, postId.ToString())
                .Get();

            return result.Models.Select(like => like.UserId).ToList();
        }

    }
}
