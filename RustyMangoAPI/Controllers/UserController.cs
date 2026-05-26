using Microsoft.AspNetCore.Mvc;
using RustyMangoAPI.Interfaces;
using RustyMangoAPI.Requests;

namespace RustyMangoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("registrationUser")]
        public async Task<IActionResult> Registration([FromBody] RegisterRequest request)
        {
            return await _userService.Register(request);
        }

        [HttpPost("authUser")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            return await _userService.Login(request);
        }

        [HttpGet("getUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            return await _userService.GetUser(id);
        }
    }
}