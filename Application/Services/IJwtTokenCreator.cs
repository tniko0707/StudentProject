using Domain.Models;

namespace Application.Services
{
    public interface IJwtTokenCreator
    {
        public string CreateToken(User user);
    }
}