using RustyMangoApi.Models;

namespace RustyMangoAPI.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
