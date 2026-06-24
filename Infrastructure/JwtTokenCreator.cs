using Application.Services;
using Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure
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
                new Claim("role", user.Role.ToString()),
                new Claim("login", user.Login),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));
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

