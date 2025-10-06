using Blog_app_backend.Models;
using Blog_app_backend.Supabase;
using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog_app_backend.Services
{
    public class UserFollowService
    {
        private readonly Client _supabase;
        private readonly NotificationService _notificationService;

        public UserFollowService(SupabaseClientProvider clientProvider,NotificationService notificationService)
        {
            _supabase = clientProvider.GetClient();
            _notificationService = notificationService;
        }

        // Follow a user
        public async Task<bool> FollowUser(Guid followerId, Guid followingId)
        {
            if (followerId == followingId) return false; // cannot follow self

            var exists = await _supabase.From<UserFollow>()
                .Where(f => f.FollowerId == followerId && f.FollowingId == followingId)
                .Get();

            if (exists.Models.Any()) return false;

            var follow = new UserFollow
            {
                Id = Guid.NewGuid(),
                FollowerId = followerId,
                FollowingId = followingId,
                FollowedAt = DateTime.UtcNow
            };

            await _supabase.From<UserFollow>().Insert(follow);
            // 🔹 Trigger notification
            var notification = new Notification
            {
                Type = "follow",
                Content = $"You have a new follower!",
                TargetUserId = followingId,
                ReferenceId = followerId, // who followed
                ReferenceType = "user"
            };

            try
            {
                await _notificationService.CreateNotificationAsync(notification);
            }
            catch
            {
                // ignore notification errors
            }
            return true;
        }

        // Unfollow a user
        public async Task<bool> UnfollowUser(Guid followerId, Guid followingId)
        {
            try
            {
                await _supabase.From<UserFollow>()
                    .Where(f => f.FollowerId == followerId && f.FollowingId == followingId)
                    .Delete();
                // 🔹 Trigger unfollow notification (optional)
                var notification = new Notification
                {
                    Type = "unfollow",
                    Content = $"You lost a follower.",
                    TargetUserId = followingId,
                    ReferenceId = followerId,
                    ReferenceType = "user"
                };

                try
                {
                    await _notificationService.CreateNotificationAsync(notification);
                }
                catch
                {
                    // ignore notification errors
                }


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UnfollowUser] Error: {ex.Message}");
                return false;
            }
        }

        // Get followers of a user
        public async Task<List<UserFollow>> GetFollowers(Guid userId)
        {
            var followers = await _supabase.From<UserFollow>()
                .Where(f => f.FollowingId == userId)
                .Get();

            return followers?.Models?.ToList() ?? new List<UserFollow>();
        }

        // Get users that a user is following
        public async Task<List<UserFollow>> GetFollowing(Guid userId)
        {
            var following = await _supabase.From<UserFollow>()
                .Where(f => f.FollowerId == userId)
                .Get();

            return following?.Models?.ToList() ?? new List<UserFollow>();
        }

        // Check if a user is following another
        public async Task<bool> IsFollowing(Guid followerId, Guid followingId)
        {
            var exists = await _supabase.From<UserFollow>()
                .Where(f => f.FollowerId == followerId && f.FollowingId == followingId)
                .Get();

            return exists.Models.Any();
        }

        // Optional: get follower count
        public async Task<int> GetFollowerCount(Guid userId)
        {
            var followers = await GetFollowers(userId);
            return followers.Count;
        }

        // Optional: get following count
        public async Task<int> GetFollowingCount(Guid userId)
        {
            var following = await GetFollowing(userId);
            return following.Count;
        }
    }
}
