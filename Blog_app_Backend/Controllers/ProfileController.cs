using Blog_app_backend.Models;
using Blog_app_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Blog_app_backend.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileService _profileService;
        private readonly PostService _postService;

        public ProfileController(ProfileService profileService, PostService postService)
        {
            _profileService = profileService;
            _postService = postService;
        }

        // Utility: get current user ID
        private Guid GetUserId()
        {
            var sub = User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(sub)) throw new UnauthorizedAccessException();
            return Guid.Parse(sub);
        }

        // GET: /api/profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserId();
            var profile = await _profileService.GetMyProfileAsync(userId);
            if (profile == null) return NotFound(new { Message = "Profile not found" });

            // ✅ Use grouped posts (published, draft, scheduled)
            var postsGrouped = await _postService.GetMyPostsAsync(userId.ToString());

            return Ok(new ProfileWithGroupedPostsDto
            {
                Profile = MapToResponseDto(profile),
                Posts = postsGrouped
            });
        }

        // POST: /api/profile/me
        [HttpPost("me")]
        public async Task<IActionResult> CreateMyProfile([FromBody] ProfileCreateDto dto)
        {
            var profile = new Profile
            {
                Id = GetUserId(),
                FullName = dto.FullName,
                Username = dto.Username,
                Role = dto.Role,
                Bio = dto.Bio,
                Website = dto.Website,
                Twitter = dto.Twitter,
                LinkedIn = dto.LinkedIn,
                Instagram = dto.Instagram
            };

            var created = await _profileService.CreateMyProfileAsync(profile);
            return Ok(MapToResponseDto(created));
        }

        // PUT: /api/profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] ProfileUpdateDto dto)
        {
            var userId = GetUserId();
            var existing = await _profileService.GetMyProfileAsync(userId);
            if (existing == null) return NotFound(new { Message = "Profile not found" });

            var profile = new Profile
            {
                Id = existing.Id,
                FullName = dto.FullName ?? existing.FullName,
                Username = dto.Username ?? existing.Username,
                Role = dto.Role ?? existing.Role,
                Bio = dto.Bio ?? existing.Bio,
                Website = dto.Website ?? existing.Website,
                Twitter = dto.Twitter ?? existing.Twitter,
                LinkedIn = dto.LinkedIn ?? existing.LinkedIn,
                Instagram = dto.Instagram ?? existing.Instagram,
                AvatarUrl = existing.AvatarUrl
            };

            var updated = await _profileService.UpdateMyProfileAsync(userId, profile);
            return Ok(MapToResponseDto(updated));
        }

        // DELETE: /api/profile/me
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMyProfile()
        {
            await _profileService.DeleteMyProfileAsync(GetUserId());
            return NoContent();
        }

        // POST: /api/profile/me/avatar
        [HttpPost("me/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file uploaded" });

            var url = await _profileService.UploadAvatarAsync(GetUserId(), file);
            return Ok(new { AvatarUrl = url });
        }

        // DELETE: /api/profile/me/avatar/{fileName}
        [HttpDelete("me/avatar/{fileName}")]
        public async Task<IActionResult> RemoveAvatar([FromRoute] string fileName)
        {
            await _profileService.RemoveAvatarAsync(GetUserId(), fileName);
            return NoContent();
        }

        // GET: /api/profile/{userId} - fetch any user's profile with published posts only
        [HttpGet("{userId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfileById(Guid userId)
        {
            var profile = await _profileService.GetProfileByIdAsync(userId);
            if (profile == null)
                return NotFound(new { Message = "Profile not found" });

            var posts = await _postService.GetAllPostsByUserId(userId.ToString());

            return Ok(new ProfileWithPostsDto
            {
                Profile = MapToResponseDto(profile),
                Posts = posts
            });
        }

        // Map Profile to Response DTO
        private ProfileResponseDto MapToResponseDto(Profile profile)
        {
            return new ProfileResponseDto
            {
                Id = profile.Id ?? Guid.Empty,
                FullName = profile.FullName,
                Username = profile.Username,
                Role = profile.Role,
                AvatarUrl = profile.AvatarUrl,
                Bio = profile.Bio,
                Website = profile.Website,
                Twitter = profile.Twitter,
                LinkedIn = profile.LinkedIn,
                Instagram = profile.Instagram,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt,
                Email = User?.FindFirst("email")?.Value
            };
        }
    }

    // DTO wrapper for profile + grouped posts
    public class ProfileWithGroupedPostsDto
    {
        public ProfileResponseDto Profile { get; set; }
        public Dictionary<string, List<PostDto>> Posts { get; set; } = new();
    }

    // DTO wrapper for profile + flat posts (other users only)
    public class ProfileWithPostsDto
    {
        public ProfileResponseDto Profile { get; set; }
        public List<PostDto> Posts { get; set; }
    }
}
