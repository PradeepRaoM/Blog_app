using Blog_app_backend.Models;
using Blog_app_backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Blog_app_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly CommentService _commentService;

        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
        }

        // Create a new comment
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommentCreateDto dto)
        {
            if (dto == null || dto.PostId == Guid.Empty)
                return BadRequest("Invalid comment data.");
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Comment content cannot be empty.");

            try
            {
                var created = await _commentService.CreateCommentAsync(dto);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Update an existing comment (only by owner)
        [HttpPut("{commentId:guid}/user/{userId:guid}")]
        public async Task<IActionResult> Update(Guid commentId, Guid userId, [FromBody] CommentUpdateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Invalid comment update request.");

            var updated = await _commentService.UpdateCommentAsync(commentId, userId, request.Content);
            if (updated == null)
                return NotFound("Comment not found or not owned by user.");

            return Ok(updated);
        }

        // Delete a comment (only by owner)
        [HttpDelete("{commentId:guid}/user/{userId:guid}")]
        public async Task<IActionResult> Delete(Guid commentId, Guid userId)
        {
            var deleted = await _commentService.DeleteCommentAsync(commentId, userId);
            if (!deleted)
                return NotFound("Comment not found or not owned by user.");

            return NoContent();
        }

        // Get all comments for a post
        [HttpGet("post/{postId:guid}")]
        public async Task<IActionResult> GetByPost(Guid postId)
        {
            var comments = await _commentService.GetCommentsByPostIdAsync(postId);
            return Ok(comments);
        }

        // Like a comment
        [HttpPost("{commentId:guid}/like")]
        public async Task<IActionResult> Like(Guid commentId)
        {
            var updated = await _commentService.LikeCommentAsync(commentId);
            if (updated == null)
                return NotFound("Comment not found.");

            return Ok(updated);
        }

        // Dislike a comment
        [HttpPost("{commentId:guid}/dislike")]
        public async Task<IActionResult> Dislike(Guid commentId)
        {
            var updated = await _commentService.DislikeCommentAsync(commentId);
            if (updated == null)
                return NotFound("Comment not found.");

            return Ok(updated);
        }
    }
}
