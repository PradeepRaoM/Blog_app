using Blog_app_backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Blog_app_backend.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImageController : ControllerBase
    {
        private readonly ImageService _imageService;

        public ImageController(ImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var url = await _imageService.UploadImageAsync(file.OpenReadStream(), fileName);

            return Ok(new { url });
        }




        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteImage(string fileName)
        {
            await _imageService.DeleteImageAsync(fileName);
            return Ok(new { message = "Image deleted successfully" });
        }
    }
}
