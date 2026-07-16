using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Users.Application.Services;
using Users.Domain.Models;

namespace Users.Infrastructure.Services
{
    public class JwtTokenCreator : IJwtTokenCreator
    {
        private readonly JwtSettings _jwtSettings;
        public JwtTokenCreator(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        public string CreateToken(User user)
        {

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                signingCredentials: creds,
                expires: DateTime.Now.AddMinutes(
                    _jwtSettings.ExpirationInMinutes)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

