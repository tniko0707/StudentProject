using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTO
{
    public class AuthentificationResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Login {  get; set; } = string.Empty;  
        public string Password { get; set; } = string.Empty;
        public string Role {  get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
