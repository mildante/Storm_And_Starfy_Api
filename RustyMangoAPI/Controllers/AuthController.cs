using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StormAndStarfyApi.Interfaces;
using StormAndStarfyApi.Requests;

namespace StormAndStarfyApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            return await _userService.Register(request);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            return await _userService.Login(request);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst("sub")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { error = "Недействительный токен" });

            return await _userService.GetUser(userId);
        }
    }
}
