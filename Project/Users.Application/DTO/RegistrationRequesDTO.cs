using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTO
{
    public class RegistrationRequesDTO
    {
        [Required(ErrorMessage = "Введите логин")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage ="ВВедите почту")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage ="Введите пароль")]
        public string Password { get; set; } = string.Empty;
    }
}
