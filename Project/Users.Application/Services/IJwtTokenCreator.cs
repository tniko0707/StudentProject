using Users.Domain.Models;

namespace Users.Application.Services
{
    public interface IJwtTokenCreator
    {
        public string CreateToken(User user);
    }
}