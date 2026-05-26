using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StormAndStarfyApi.Data;
using StormAndStarfyApi.Interfaces;
using StormAndStarfyApi.Models;
using StormAndStarfyApi.Requests;

namespace StormAndStarfyApi.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            AppDbContext context,
            IJwtService jwtService,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var validationError = ValidateRegisterRequest(request);
            if (validationError != null)
                return new BadRequestObjectResult(new { error = validationError });

            var login = request.Login.Trim();
            var name = request.Name.Trim();
            var loginLower = login.ToLower();

            var loginExists = await _context.Users.AnyAsync(x => x.Login.ToLower() == loginLower);
            if (loginExists)
                return new BadRequestObjectResult(new { error = "Login is already taken" });

            var user = new User
            {
                Login = login,
                Name = name,
                CreatedAtUtc = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            return new OkObjectResult(new
            {
                token,
                user = ToUserResponse(user)
            });
        }

        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login))
                return new BadRequestObjectResult(new { error = "Login is required" });

            if (string.IsNullOrWhiteSpace(request.Password))
                return new BadRequestObjectResult(new { error = "Password is required" });

            var loginLower = request.Login.Trim().ToLower();
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Login.ToLower() == loginLower);

            if (user == null ||
                _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return new BadRequestObjectResult(new { error = "Invalid login or password" });
            }

            var token = _jwtService.GenerateToken(user);

            return new OkObjectResult(new
            {
                token,
                user = ToUserResponse(user)
            });
        }

        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return new NotFoundObjectResult(new { error = "User not found" });

            return new OkObjectResult(new
            {
                user = ToUserResponse(user)
            });
        }

        private static string? ValidateRegisterRequest(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login))
                return "Login is required";

            if (string.IsNullOrWhiteSpace(request.Name))
                return "Name is required";

            if (string.IsNullOrWhiteSpace(request.Password))
                return "Password is required";

            if (request.Password.Length < 6)
                return "Password must be at least 6 characters";

            return null;
        }

        private static object ToUserResponse(User user)
        {
            return new
            {
                user.Id,
                user.Login,
                user.Name
            };
        }
    }
}
