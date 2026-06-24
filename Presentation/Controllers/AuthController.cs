using Application.Services;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController: Controller
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenCreator _jwtTokenGenerator;

        public AuthController(IUserService userService, IJwtTokenCreator jwtTokenGenerator)
        {
            _userService = userService;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {

            var user = await _userService.FindUserByLogin(loginRequest.Login);
            if (user == null) return Unauthorized("Пользователь не найден");

            bool isPasswordValid = await _userService.VerifyPassword(user, loginRequest.Password);
            if (!isPasswordValid) return Unauthorized("Неверный пароль");

            var token = _jwtTokenGenerator.CreateToken(user);

            return Ok(new {Token = token});
        }


        [HttpPost("register")]
        public async Task<IActionResult> Registrate([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var user = await _userService.RegistrateUser(loginRequest.Login, loginRequest.Password);
                return NoContent(); 
            }
            catch (Exception ex)
            {
                return Unauthorized("Ошибка регистрации");
            }
        }

    }
}
