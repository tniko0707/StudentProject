using Microsoft.AspNetCore.Mvc;
using Users.Application.DTO;
using Users.Application.Services;
using Users.Domain.Models;

namespace Users.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            try
            {
                var response = await _userService.LoginAsync(loginRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized("Ошибка входа. Проверь пароли мароли");
            }
            //var user = await _userService.FindUserByLogin(loginRequest.Login);
            //if (user == null) return Unauthorized("Пользователь не найден");

            //bool isPasswordValid = await _userService.VerifyPassword(user, loginRequest.Password);
            //if (!isPasswordValid) return Unauthorized("Неверный пароль");

            //var token = _jwtTokenGenerator.CreateToken(user);

            //return Ok(new { Token = token });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Registrate([FromBody] RegistrationRequesDTO registrationRequest)
        {
            try
            {
                var response = await _userService.RegistrateAsync(registrationRequest);
                return Created("Зарегистрирован", response);
            }
            catch (Exception ex)
            {
                return Unauthorized("Ошибка регистрации");
            }
        }

    }
}
