using Microsoft.EntityFrameworkCore;
using RustyMangoAPI.Data;
using RustyMangoAPI.Interfaces;
using RustyMangoAPI.Models;
using RustyMangoAPI.Requests;
using Microsoft.AspNetCore.Mvc;
using RustyMangoApi.Models;

namespace RustyMangoAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public UserService(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var login_check = await _context.Users.AnyAsync(x => x.Login == request.Login);
            if (login_check)
                return new BadRequestObjectResult(new { error = "Логин уже занят" });

            var user = new User
            {
                Login = request.Login,
                Password = request.Password,
                Name = request.Name,
                TotalScore = 0
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var progressList = new List<UserProgress>
            {
                new UserProgress
                {
                    UserId = user.Id,
                    LevelNumber = 1,
                    IsUnlocked = true,
                    IsCompleted = false
                },
                new UserProgress
                {
                    UserId = user.Id,
                    LevelNumber = 2,
                    IsUnlocked = false,
                    IsCompleted = false
                },
                new UserProgress
                {
                    UserId = user.Id,
                    LevelNumber = 3,
                    IsUnlocked = false,
                    IsCompleted = false
                }
            };

            _context.UserProgresses.AddRange(progressList);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                user = new
                {
                    user.Id,
                    user.Login,
                    user.Name,
                    user.TotalScore
                }
            });
        }

        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Login == request.Login && x.Password == request.Password);

            if (user == null)
                return new BadRequestObjectResult(new { error = "Неверный логин или пароль" });

            var token = _jwtService.GenerateToken(user);

            return new OkObjectResult(new
            {
                status = true,
                token = token,
                user = new
                {
                    user.Id,
                    user.Login,
                    user.Name,
                    user.TotalScore
                }
            });
        }

        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return new BadRequestObjectResult(new { error = "Пользователь не найден" });

            return new OkObjectResult(new
            {
                status = true,
                user = new
                {
                    user.Id,
                    user.Login,
                    user.Name,
                    user.TotalScore
                }
            });
        }
    }
}