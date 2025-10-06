using Blog_app_backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Blog_app_backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService) // ✅ constructor injection
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] string email, [FromForm] string password)
        {
            var session = await _authService.RegisterAsync(email, password);
            return Ok(session);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            var session = await _authService.LoginAsync(email, password);
            return Ok(session);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromForm] string email)
        {
            await _authService.ResetPasswordAsync(email);
            return Ok(new { message = "Password reset email sent." });
        }
    }
}
