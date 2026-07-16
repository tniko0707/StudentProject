using Users.Application.DTO;
using Users.Application.Repositories;
using Users.Domain.Models;

namespace Users.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenCreator _jwtTokenCreator;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenCreator jwtTokenCreator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenCreator = jwtTokenCreator;
        }

        public async Task<AuthentificationResponseDto> RegistrateAsync(RegistrationRequesDTO registrationRequesDTO)
        {
            if (await _userRepository.GetByEmailAsync(registrationRequesDTO.Email) != null)
            {
                throw new Exception("Пользователь с таким почтовым адресом уже существует");
            }

            if (await _userRepository.GetByLoginAsync(registrationRequesDTO.Login) != null)
            {
                throw new Exception("Пользователь с таким логином уже существует");
            }

            var user = new User(registrationRequesDTO.Login,
                registrationRequesDTO.Email,
                _passwordHasher.HashPassword(registrationRequesDTO.Password),
                Role.Admin);
            await _userRepository.AddAsync(user);

            var token = _jwtTokenCreator.CreateToken(user);
            return new AuthentificationResponseDto
            {
                Token = token,
                Email = user.Email,
                Login = user.Login,
                Role = user.Role.ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        public async Task<AuthentificationResponseDto> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            var user = await _userRepository.GetByEmailAsync(loginRequestDTO.Email)??
                throw new UnauthorizedAccessException("Пользователь с таким Email не найден");

            if (!_passwordHasher.VerifyPassword(loginRequestDTO.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Пароль ты выбрал не тот");
            }

            var token = _jwtTokenCreator.CreateToken(user);
            return new AuthentificationResponseDto
            {
                Token = token,
                Email = user.Email,
                Login = user.Login,
                Role = user.Role.ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

        }

        public async Task<User> FindUserByLogin(string username)
        {
            return await _userRepository.GetByLoginAsync(username);
        }

        public async Task<bool> VerifyPassword(User user, string password)
        {
            return _passwordHasher.VerifyPassword(password, user.PasswordHash);
        }

        //public async Task<User> RegistrateUser(string username, string password)
        //{
        //    if (await _userRepository.GetByLoginAsync(username) != null)
        //    {
        //        throw new Exception("Пользователь с таким именем существует");
        //    }
        //    var user = new User
        //    {
        //        Login = username,
        //        Role = Role.Admin,
        //    };

        //    user.PasswordHash = _passwordHasher.HashPassword(password);

        //    await _userRepository.AddAsync(user);
        //    return user;
        //}

        public async Task<User> FindByIdAsync(Guid userId, CancellationToken ct)
        {
            return await _userRepository.GetByIdAsync(userId, ct);
        }



    }
}

