using Microsoft.AspNetCore.Mvc;
using RustyMangoAPI.Models;
using RustyMangoAPI.Requests;

namespace RustyMangoAPI.Interfaces
{
    public interface IUserService
    {
        Task<IActionResult> Register(RegisterRequest request);
        Task<IActionResult> Login(LoginRequest request);
        Task<IActionResult> GetUser(int id);
    }
}
