using Users.Application.DTO;
using Users.Domain.Models;

namespace Users.Application.Services
{
    public interface IUserService
    {
        Task<AuthentificationResponseDto> RegistrateAsync(RegistrationRequesDTO registrationRequesDTO);
        Task<AuthentificationResponseDto> LoginAsync(LoginRequestDTO loginRequestDTO);
        //Task<User> RegistrateUser(string username, string password);
        Task<User> FindByIdAsync(Guid guid, CancellationToken ct = default);
        Task<User> FindUserByLogin(string username);
        Task<bool> VerifyPassword(User user, string password);
    }
}
