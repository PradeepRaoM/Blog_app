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
    [Route("api/tags")]
    public class TagController : ControllerBase
    {
        private readonly TagService _service;

        public TagController(TagService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTag([FromBody] TagDto dto)
        {
            var tag = new Tag
            {
                Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = dto.CreatedAt != default ? dto.CreatedAt : DateTime.UtcNow
            };

            var created = await _service.CreateTagAsync(tag);

            // Return a clean DTO (no Supabase attributes)
            return Ok(new TagDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                CreatedAt = created.CreatedAt
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _service.GetAllTagsAsync();

            var result = tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var tag = await _service.GetTagByIdAsync(id);
            if (tag == null)
                return NotFound();

            return Ok(new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Description = tag.Description,
                CreatedAt = tag.CreatedAt
            });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteTagAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}
