using StormAndStarfyApi.Models;

namespace StormAndStarfyApi.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
