using Blog_app_backend.Models;
using Blog_app_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Blog_app_backend.Controllers
{
    [ApiController]
    [Route("api/follow")]
    [Authorize]
    public class FollowController : ControllerBase
    {
        private readonly UserFollowService _service;

        public FollowController(UserFollowService service)
        {
            _service = service;
        }

        private Guid GetCurrentUserId()
        {
            var sub = User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(sub)) throw new UnauthorizedAccessException();
            return Guid.Parse(sub);
        }

        // POST: /api/follow/{userId}
        [HttpPost("{userId:guid}")]
        public async Task<IActionResult> Follow(Guid userId)
        {
            var currentUserId = GetCurrentUserId();
            var success = await _service.FollowUser(currentUserId, userId);
            if (!success) return BadRequest(new { message = "Cannot follow user" });
            return Ok(new { message = "Followed successfully" });
        }

        // POST: /api/follow/unfollow/{userId}
        [HttpPost("unfollow/{userId:guid}")]
        public async Task<IActionResult> Unfollow(Guid userId)
        {
            var currentUserId = GetCurrentUserId();
            var success = await _service.UnfollowUser(currentUserId, userId);
            if (!success) return BadRequest(new { message = "Cannot unfollow user" });
            return Ok(new { message = "Unfollowed successfully" });
        }

        // GET: /api/follow/followers/{userId}
        [HttpGet("followers/{userId:guid}")]
        public async Task<IActionResult> GetFollowers(Guid userId)
        {
            var followers = await _service.GetFollowers(userId);
            var followerIds = followers.Select(f => f.FollowerId).ToList();
            return Ok(followerIds);
        }

        // GET: /api/follow/following/{userId}
        [HttpGet("following/{userId:guid}")]
        public async Task<IActionResult> GetFollowing(Guid userId)
        {
            var following = await _service.GetFollowing(userId);
            var followingIds = following.Select(f => f.FollowingId).ToList();
            return Ok(followingIds);
        }

        // GET: /api/follow/isfollowing/{userId}
        [HttpGet("isfollowing/{userId:guid}")]
        public async Task<IActionResult> IsFollowing(Guid userId)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.IsFollowing(currentUserId, userId);
            return Ok(new { isFollowing = result }); // lowercase key
        }
    }
}
