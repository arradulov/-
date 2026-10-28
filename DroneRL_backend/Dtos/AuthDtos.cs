using System;
using System.ComponentModel.DataAnnotations;
using DroneRL_backend.Models;

namespace DroneRL_backend.Dtos
{
    public class LoginDto
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class TokenDto
    {
        public string Token { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    // Регистрация нового пользователя (доступна только администратору)
    public class RegisterDto
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string Login { get; set; }

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [RegularExpression("^(admin|user)$", ErrorMessage = "Role must be 'admin' or 'user'")]
        public string Role { get; set; } = Roles.User;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }

        public static UserDto From(User u) => new UserDto { Id = u.Id, Login = u.Login, Role = u.Role };
    }
}
