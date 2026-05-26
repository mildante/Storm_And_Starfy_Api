using Microsoft.AspNetCore.Mvc;
using RustyMangoAPI.Requests;
using RustyMangoAPI.Models;

namespace RustyMangoAPI.Interfaces
{
    public interface IGameService
    {
        Task<IActionResult> GetProgress(int userId);
        Task<IActionResult> CompleteLevel(CompleteLevelRequest request);
        Task<IActionResult> GetLeaderboard();
    }
}
