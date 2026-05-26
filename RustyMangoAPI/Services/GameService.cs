using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RustyMangoAPI.Data;
using RustyMangoAPI.Interfaces;
using RustyMangoAPI.Models;
using RustyMangoAPI.Requests;

namespace RustyMangoAPI.Services
{
    public class GameService : IGameService
    {
        private readonly AppDbContext _context;

        public GameService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetProgress(int userId)
        {
            var progress = await _context.UserProgresses
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.LevelNumber)
                .ToListAsync();
            if (progress == null || progress.Count == 0)
                return new NotFoundObjectResult(new { error = "Прогресс пользователя не найден" });
            return new OkObjectResult(new
            {
                status = true,
                progress
            });
        }

        public async Task<IActionResult> CompleteLevel(CompleteLevelRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId);
            if (user == null)
                return new NotFoundObjectResult(new { error = "Пользователь не найден" });

            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.LevelNumber == request.LevelNumber);

            if (progress == null)
                return new NotFoundObjectResult(new { error = "Прогресс пользователя не найден" });

            progress.IsCompleted = true;
            user.TotalScore += request.Score;

            var nextLevel = await _context.UserProgresses
                .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.LevelNumber == request.LevelNumber + 1);

            if (nextLevel != null)
                nextLevel.IsUnlocked = true;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                status = true,
                totalScore = user.TotalScore
            });
        }

        public async Task<IActionResult> GetLeaderboard()
        {
            var users = await _context.Users
                .OrderByDescending(x => x.TotalScore)
                .Select(x => new
                {
                    x.Name,
                    x.TotalScore
                })
                .ToListAsync();

            return new OkObjectResult(new
            {
                status = true,
                leaderboard = users
            });
        }
    }
}