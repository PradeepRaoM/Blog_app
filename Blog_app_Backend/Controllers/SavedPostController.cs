using Blog_app_backend.Models;
using Blog_app_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blog_app_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SavedPostController : ControllerBase
    {
        private readonly SavedPostService _savedPostService;

        public SavedPostController(SavedPostService savedPostService)
        {
            _savedPostService = savedPostService;
        }

        [HttpPost("save/{postId}")]
        public async Task<IActionResult> SavePost(Guid postId, [FromQuery] Guid? collectionId = null)
        {
            try
            {
                var userId = GetUserId();
                var savedPostDto = await _savedPostService.SavePostAsync(postId, userId, collectionId);
                return Ok(savedPostDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("remove/{postId}")]
        public async Task<IActionResult> RemoveSavedPost(Guid postId)
        {
            try
            {
                var userId = GetUserId();
                var success = await _savedPostService.RemoveSavedPostAsync(postId, userId);

                if (!success)
                    return NotFound(new { message = "Saved post not found." });

                return Ok(new { message = "Saved post removed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSavedPosts([FromQuery] Guid? collectionId = null)
        {
            try
            {
                var userId = GetUserId();
                var savedPostDtos = await _savedPostService.GetSavedPostsAsync(userId, collectionId);
                return Ok(savedPostDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("collection")]
        public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                return BadRequest(new { message = "Collection name cannot be empty." });

            try
            {
                var userId = GetUserId();
                var collection = await _savedPostService.CreateCollectionAsync(userId, request.Name);
                return Ok(collection);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("collection")]
        public async Task<IActionResult> GetCollections()
        {
            try
            {
                var userId = GetUserId();
                var collections = await _savedPostService.GetCollectionsAsync(userId);
                return Ok(collections);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("collection/{collectionId}")]
        public async Task<IActionResult> UpdateCollection(Guid collectionId, [FromBody] string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return BadRequest(new { message = "New collection name cannot be empty." });

            try
            {
                var userId = GetUserId();
                var updatedCollection = await _savedPostService.UpdateCollectionAsync(collectionId, userId, newName);

                if (updatedCollection == null)
                    return NotFound(new { message = "Collection not found or no permission." });

                return Ok(updatedCollection);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("collection/{collectionId}")]
        public async Task<IActionResult> DeleteCollection(Guid collectionId)
        {
            try
            {
                var userId = GetUserId();
                var success = await _savedPostService.DeleteCollectionAsync(collectionId, userId);

                if (!success)
                    return NotFound(new { message = "Collection not found or no permission." });

                return Ok(new { message = "Collection deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new Exception("User ID not found in token.");

            return Guid.Parse(userIdClaim);
        }
    }

    public class CreateCollectionRequest
    {
        public string Name { get; set; }
    }
}