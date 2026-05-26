using Microsoft.IdentityModel.Tokens;
using StormAndStarfyApi.Interfaces;
using StormAndStarfyApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StormAndStarfyApi.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim("login", user.Login),
                new Claim("name", user.Name)
            };

            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var parsedMinutes)
                ? parsedMinutes
                : 60;

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
