using Microsoft.AspNetCore.Mvc;
using StormAndStarfyApi.Requests;

namespace StormAndStarfyApi.Interfaces
{
    public interface IUserService
    {
        Task<IActionResult> Register(RegisterRequest request);
        Task<IActionResult> Login(LoginRequest request);
        Task<IActionResult> GetUser(int id);
    }
}
