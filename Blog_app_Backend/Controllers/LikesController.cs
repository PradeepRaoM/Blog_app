using Blog_app_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blog_app_backend.Controllers
{
    [ApiController]
    [Route("api/likes")]
    public class LikesController : ControllerBase
    {
        private readonly LikeService _likeService;

        public LikesController(LikeService likeService)
        {
            _likeService = likeService;
        }

        private Guid? GetUserId()
        {
            var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(sub, out var userId))
                return userId;

            return null;
        }

        // POST: /api/likes/{postId}/like
        [HttpPost("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> Like(Guid postId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid user." });

            var result = await _likeService.LikeAsync(postId, userId.Value);
            return Ok(new { liked = result });
        }

        // POST: /api/likes/{postId}/dislike
        [HttpPost("{postId}/dislike")]
        [Authorize]
        public async Task<IActionResult> Dislike(Guid postId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid user." });

            var result = await _likeService.RemoveLikeAsync(postId, userId.Value);
            return Ok(new { disliked = result });
        }

        // GET: /api/likes/{postId}/count
        [HttpGet("{postId}/count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLikesCount(Guid postId)
        {
            var count = await _likeService.GetLikesCountAsync(postId);
            return Ok(new { count });
        }

        // GET: /api/likes/{postId}/status
        [HttpGet("{postId}/status")]
        [Authorize]
        public async Task<IActionResult> HasUserLiked(Guid postId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid user." });

            var liked = await _likeService.HasUserLikedAsync(postId, userId.Value);
            return Ok(new { liked });
        }
    }
}
